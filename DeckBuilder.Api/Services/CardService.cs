using System.Text.RegularExpressions;
using DeckBuilder.Api.Enums;
using DeckBuilder.Api.Exceptions;
using DeckBuilder.Api.Models;
using DeckBuilder.Api.ViewModels;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace DeckBuilder.Api.Services;

public class CardService(IMongoDatabase database)
{
    private readonly IMongoCollection<Card> _collection = database.GetCollection<Card>(Collections.Cards);

    public async Task<int> CountNumberOfCards(CancellationToken cancellationToken)
    {
        return await _collection.AsQueryable().CountAsync(cancellationToken);
    }

    public async Task<CardOutput> GetCardById(string cardId, CancellationToken cancellationToken)
    {
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

        return BsonSerializer.Deserialize<CardOutput>(result.ToBsonDocument());
    }

    public async Task<CardOutput> GetCardBySetAndCode(string setCode, string code, CancellationToken cancellationToken)
    {
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
        
        return BsonSerializer.Deserialize<CardOutput>(result.ToBsonDocument());
    }

    public async Task<IEnumerable<CardOutput>> GetCardsBySet(string setCode, CancellationToken cancellationToken)
    {
        var pipeline = new[]
        {
            CardServiceDocumentHelpers.MatchBySet(setCode),
            CardServiceDocumentHelpers.ProjectCardWithDetails()
        };
        
        var results = await _collection
            .Aggregate<BsonDocument>(pipeline, cancellationToken: cancellationToken)
            .ToListAsync(cancellationToken);
        
        return results.Select(c => BsonSerializer.Deserialize<CardOutput>(c.ToBsonDocument()));
    }
    
    public async Task<IEnumerable<CardOutput>> SearchCards(string? query, CancellationToken cancellationToken)
    {
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

        return result.Select(c => c.ToCardOutput());
    }
    
    public async Task<IEnumerable<CardOutput>> GetCardDetailsByIds(IEnumerable<string> cardIds, CancellationToken cancellationToken)
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