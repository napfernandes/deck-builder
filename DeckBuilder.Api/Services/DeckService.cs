using DeckBuilder.Api.Enums;
using DeckBuilder.Api.Exceptions;
using DeckBuilder.Api.Models;
using DeckBuilder.Api.ViewModels;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace DeckBuilder.Api.Services;

public class DeckService(IMongoDatabase database)
{
    private readonly IMongoCollection<Deck> _collection = database.GetCollection<Deck>(Collections.Decks);
    
    public async ValueTask<IEnumerable<DeckOutput>> SearchDecks(CancellationToken cancellationToken)
    {
        var pipeline = new[]
        {
            DeckServiceDocumentHelpers.LookupCreatedByUsers(),
            DeckServiceDocumentHelpers.SetCreatedByUser(),
            DeckServiceDocumentHelpers.UnsetCreatedAtUsers()
        };
        
        var result = await _collection
            .Aggregate<BsonDocument>(pipeline, cancellationToken: cancellationToken)
            .ToListAsync(cancellationToken);

        return result.Select(deck => BsonSerializer.Deserialize<DeckOutput>(deck.ToBsonDocument()));
    }
    
    public async ValueTask<DeckOutput> GetDeckById(string deckId, CancellationToken cancellationToken)
    {
        var pipeline = new[]
        {
            DeckServiceDocumentHelpers.MatchById(deckId),
            DeckServiceDocumentHelpers.LookupCreatedByUsers(),
            DeckServiceDocumentHelpers.SetCreatedByUser(),
            DeckServiceDocumentHelpers.UnsetCreatedAtUsers()
        };

        var result = await _collection
            .Aggregate<BsonDocument>(pipeline, cancellationToken: cancellationToken)
            .FirstOrDefaultAsync(cancellationToken);
        
        if (result is null)
            throw KnownException.DeckNotFound(deckId);
        
        return BsonSerializer.Deserialize<DeckOutput>(result.ToBsonDocument());
    }
}

public static class DeckServiceDocumentHelpers
{
    public static BsonDocument MatchById(string deckId)
    {
        return new BsonDocument
        {
            {
                "$match", new BsonDocument
                {
                    { "_id", new ObjectId(deckId) }
                }
            }
        };
    }

    public static BsonDocument LookupCreatedByUsers()
    {
        return new BsonDocument("$lookup", new BsonDocument
        {
            { "from", "users" },
            { "let", new BsonDocument { { "createdBy", "$createdBy" } } },
            { "pipeline", new BsonArray
                {
                    new BsonDocument("$match", new BsonDocument
                    {
                        { "$expr", new BsonDocument("$eq", new BsonArray { "$_id", "$$createdBy" }) }
                    }),
                    new BsonDocument("$limit", 1),
                    new BsonDocument("$project", new BsonDocument
                    {
                        { "_id", 1 },
                        { "firstName", 1 },
                        { "lastName", 1 },
                        { "email", 1 }
                    })
                }
            },
            { "as", "createdUsers" }
        });
    }

    public static BsonDocument SetCreatedByUser()
    {
        return new BsonDocument("$set", new BsonDocument
        {
            {
                "createdByUser", new BsonDocument("$arrayElemAt", new BsonArray { "$createdUsers", 0 })
            }
        });
    }

    public static BsonDocument UnsetCreatedAtUsers()
    {
        return new BsonDocument("$unset", "createdUsers");
    }
}