using System.Text.RegularExpressions;
using DeckBuilder.Api.Enums;
using DeckBuilder.Api.Exceptions;
using DeckBuilder.Api.Helpers;
using DeckBuilder.Api.Models;
using DeckBuilder.Api.ViewModels;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace DeckBuilder.Api.Services;

public class CardService(IMongoDatabase database)
{
    private readonly IMongoCollection<Card> _collection = database.GetCollection<Card>(Collections.Cards);

    public async ValueTask<int> CountNumberOfCards(CancellationToken cancellationToken)
    {
        var countResult = CacheManager.GetItem<int?>(CacheKeys.CountCards);

        if (countResult.HasValue)
            return countResult.Value;
        
        countResult = await _collection.AsQueryable().CountAsync(cancellationToken);
        
        CacheManager.SetItem(CacheKeys.CountCards, countResult, TimeSpan.FromSeconds(5));
        return countResult.Value;
    }

    public async ValueTask<CardOutput> GetCardById(string cardId, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.CardById(cardId);
        var cardOutput = CacheManager.GetItem<CardOutput?>(cacheKey);

        if (cardOutput is not null)
            return cardOutput;
        
        var pipeline = new[]
        {
            CardServiceDocumentHelpers.MatchById(cardId),
            CardServiceDocumentHelpers.ProjectCardWithDetails()
        };

        var result = await _collection
            .Aggregate<BsonDocument>(pipeline, cancellationToken: cancellationToken)
            .FirstOrDefaultAsync(cancellationToken);
        
        if (result is null)
            throw KnownException.CardNotFoundById(cardId);

        var convertedResult = BsonSerializer.Deserialize<CardOutput>(result.ToBsonDocument());
        CacheManager.SetItem(cacheKey, convertedResult);

        return convertedResult;
    }

    public async ValueTask<CardOutput> GetCardBySetAndCode(string setCode, string code, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.CardBySetAndCode(setCode, code);
        var cardOutput = CacheManager.GetItem<CardOutput?>(cacheKey);

        if (cardOutput is not null)
            return cardOutput;
        
        var pipeline = new[]
        {
            CardServiceDocumentHelpers.MatchBySetCodeAndCode(setCode, code),
            CardServiceDocumentHelpers.ProjectCardWithDetails()
        };
        
        var result = await _collection
            .Aggregate<BsonDocument>(pipeline, cancellationToken: cancellationToken)
            .FirstOrDefaultAsync(cancellationToken);
        
        if (result is null)
            throw KnownException.CardNotFoundBySetAndCode(setCode, code);
        
        var convertedResult = BsonSerializer.Deserialize<CardOutput>(result.ToBsonDocument());
        CacheManager.SetItem(cacheKey, convertedResult);

        return convertedResult;
    }

    public async ValueTask<IEnumerable<CardOutput>> GetCardsBySet(string setCode, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.CardsBySet(setCode);
        var listOutput = CacheManager.GetItem<IEnumerable<CardOutput>>(cacheKey);

        if (listOutput is not null)
            return listOutput;

        var pipeline = new[]
        {
            CardServiceDocumentHelpers.MatchBySet(setCode),
            CardServiceDocumentHelpers.ProjectCardWithDetails()
        };
        
        var results = await _collection
            .Aggregate<BsonDocument>(pipeline, cancellationToken: cancellationToken)
            .ToListAsync(cancellationToken);
        
        var convertedList = results.Select(c => BsonSerializer.Deserialize<CardOutput>(c.ToBsonDocument()));
        CacheManager.SetItem(cacheKey, convertedList);
        
        return convertedList;
    }
    
    public async ValueTask<IEnumerable<CardOutput>> SearchCards(string? query, CancellationToken cancellationToken)
    {
        var cacheKey = query is not null ? CacheKeys.CardsSearchByQuery(query) : string.Empty;
        if (query is not null)
        {
            var listOutput = CacheManager.GetItem<IEnumerable<CardOutput>>(cacheKey);

            if (listOutput is not null)
                return listOutput;
        }
        
        var collection = _collection.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var regex = new Regex(query, RegexOptions.IgnoreCase);
            collection = collection.Where(c =>
                c.Attributes.Any(a => a.Searchable &&
                   a.Values != null && a.Values.Any() ?
                    a.Values.Any(v => regex.IsMatch(v))
                    : regex.IsMatch(a.Value!)));
        }

        var result = await collection.ToListAsync(cancellationToken);
        var convertedList = result.Select(c => c.ToCardOutput());
        
        if (!string.IsNullOrWhiteSpace(cacheKey))
            CacheManager.SetItem(cacheKey, convertedList);

        return convertedList;
    }
    
    public async ValueTask<IEnumerable<CardOutput>> GetCardDetailsByIds(IEnumerable<string> cardIds, CancellationToken cancellationToken)
    {
        var pipeline = new[]
        {
            CardServiceDocumentHelpers.MatchIdsInArray(cardIds),
            CardServiceDocumentHelpers.ProjectCardWithDetails()
        };

        var results = await _collection
            .Aggregate<BsonDocument>(pipeline, cancellationToken: cancellationToken)
            .ToListAsync(cancellationToken);

        return results.Select(c => BsonSerializer.Deserialize<CardOutput>(c.ToBsonDocument()));
    }
}

public static class CardServiceDocumentHelpers
{
    public static BsonDocument MatchById(string cardId)
    {
        return new BsonDocument
        {
            {
                "$match", new BsonDocument
                {
                    { "_id", new ObjectId(cardId) }
                }
            }
        };
    }

    public static BsonDocument MatchBySetCodeAndCode(string setCode, string code)
    {
        return new BsonDocument("$match", new BsonDocument
        {
            {
                "$and", new BsonArray
                {
                    new BsonDocument
                    {
                        { "attributes.key", "setCode" },
                        { "attributes.value", setCode }
                    },
                    new BsonDocument
                    {
                        { "attributes.key", "code" },
                        { "attributes.value", code }
                    }
                }
            }
        });
    }
    
    public static BsonDocument MatchBySet(string setCode)
    {
        return new BsonDocument
        {
            {
                "$match", new BsonDocument
                {
                    { "attributes.key", "setCode" },
                    { "attributes.value", setCode }
                }
            }
        };
    }
    
    public static BsonDocument MatchIdsInArray(IEnumerable<string> cardIds)
    {
        return new BsonDocument
        {
            {
                "$match", new BsonDocument
                {
                    {
                        "_id", new BsonDocument
                        {
                            { "$in", new BsonArray(cardIds.Select(id => new ObjectId(id))) }
                        }
                    }
                }
            }
        };
    }
    
    public static BsonDocument ProjectCardWithDetails()
    {
        return new BsonDocument
        {
            {
                "$project", new BsonDocument
                {
                    { "_id", 1 },
                    { "language", 1 },
                    {
                        "attributes", new BsonDocument("$arrayToObject", new BsonDocument("$map", new BsonDocument
                        {
                            { "input", "$attributes" },
                            { "as", "attr" },
                            {
                                "in", new BsonArray
                                {
                                    new BsonDocument("$toString", "$$attr.key"),
                                    new BsonDocument("$cond", new BsonDocument
                                    {
                                        {
                                            "if", new BsonDocument("$eq", new BsonArray
                                            {
                                                new BsonDocument("$size", "$$attr.values"),
                                                0
                                            })
                                        },
                                        { "then", "$$attr.value" },
                                        { "else", "$$attr.values" }
                                    })
                                }
                            }
                        }))
                    }
                }
            }
        };
    }
}