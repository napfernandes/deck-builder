using System.Text.RegularExpressions;
using DeckBuilder.Api.Enums;
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
        var pipeline = new BsonDocument[]
        {
            new("$match", CardServiceDocumentHelpers.MatchById(cardId)),
            new("$project", CardServiceDocumentHelpers.ProjectCard())
        };

        var result = await _collection
            .Aggregate<BsonDocument>(pipeline, cancellationToken: cancellationToken)
            .FirstOrDefaultAsync(cancellationToken);
        
        return BsonSerializer.Deserialize<CardOutput>(result.ToBsonDocument());
    }

    public async Task<IEnumerable<CardOutput>> SearchCards(string? query, CancellationToken cancellationToken)
    {
        var collection = _collection.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var regex = new Regex(query, RegexOptions.IgnoreCase);
            collection = collection.Where(c => c.Attributes.Any(a => a.Searchable && regex.IsMatch(a.Value)));
        }

        var result = await collection.ToListAsync(cancellationToken);

        return result.Select(c => c.ToCardOutput());
    }
}

public static class CardServiceDocumentHelpers
{
    public static BsonDocument MatchById(string cardId)
    {
        return new BsonDocument
        {
            { "_id", new ObjectId(cardId) }
        };
    }

    public static BsonDocument ProjectCard()
    {
        return new BsonDocument
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
                                { "if", new BsonDocument("$eq", new BsonArray
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
        };
    }
}