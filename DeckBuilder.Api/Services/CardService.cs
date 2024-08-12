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
            CardServiceMongoPipelines.MatchById(cardId),
            CardServiceMongoPipelines.ProjectCardWithDetails()
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
            CardServiceMongoPipelines.MatchBySetCode(setCode),
            CardServiceMongoPipelines.MatchByCode(code),
            CardServiceMongoPipelines.ProjectCardWithDetails()
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
            CardServiceMongoPipelines.MatchBySet(setCode),
            CardServiceMongoPipelines.ProjectCardWithDetails()
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
            var criteria = GetQueryCriteria(query).ToList();
            if (criteria.Any())
            {
                foreach (var item in criteria)
                {
                    var regex = new Regex(item.Value, RegexOptions.IgnoreCase);
                    collection = collection.Where(c =>
                        c.Attributes.Any(a => a.Searchable && a.Key == item.Key &&
                            (a.Values != null && a.Values.Any()
                                ? a.Values.Any(v => regex.IsMatch(v))
                                : regex.IsMatch(a.Value!)
                            )
                        )
                    );
                }
            }
            else
            {
                var regex = new Regex(query, RegexOptions.IgnoreCase);
                collection = collection.Where(c =>
                    c.Attributes.Any(a => a.Searchable &&
                                          a.Values != null && a.Values.Any()
                        ? a.Values.Any(v => regex.IsMatch(v))
                        : regex.IsMatch(a.Value!)));
            }
        }

        var result = await collection.ToListAsync(cancellationToken);
        var convertedList = result.Select(c => c.ToCardOutput());
        
        if (!string.IsNullOrWhiteSpace(cacheKey))
            CacheManager.SetItem(cacheKey, convertedList);

        return convertedList;
    }

    private static IEnumerable<SearchCriteria> GetQueryCriteria(string query)
    {
        var splitQuery = query.Split(",");
        foreach (var item in splitQuery)
        {
            var searchItem = item.Split("=");
            if (searchItem.Length > 1)
                yield return new SearchCriteria(searchItem[0], searchItem[1]);
        }
    }

    public async ValueTask<IEnumerable<CardOutput>> GetCardDetailsByIds(IEnumerable<string> cardIds, CancellationToken cancellationToken)
    {
        var pipeline = new[]
        {
            CardServiceMongoPipelines.MatchIdsInArray(cardIds),
            CardServiceMongoPipelines.ProjectCardWithDetails()
        };

        var results = await _collection
            .Aggregate<BsonDocument>(pipeline, cancellationToken: cancellationToken)
            .ToListAsync(cancellationToken);

        return results.Select(c => BsonSerializer.Deserialize<CardOutput>(c.ToBsonDocument()));
    }

    public async ValueTask<PackOutput> GenerateRandomPackForSet(string setCode, CancellationToken cancellationToken)
    {
        var pipeline = new List<BsonDocument>
        {
            CardServiceMongoPipelines.MatchBySet(setCode),
            CardServiceMongoPipelines.FacetCardsByRarities()
        };
        
        foreach (var step in CardServiceMongoPipelines.ProjectFacetToRoot())
            pipeline.Add(step);
        
        pipeline.Add(CardServiceMongoPipelines.ProjectCardWithDetails());
        
        var results = await _collection
            .Aggregate<BsonDocument>(pipeline, cancellationToken: cancellationToken)
            .ToListAsync(cancellationToken);

        return new PackOutput(results.Select(c => BsonSerializer.Deserialize<CardOutput>(c.ToBsonDocument())));
    }
}

public record SearchCriteria(string Key, string Value);
