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
    
    public async Task<IEnumerable<Deck>> SearchDecks(CancellationToken cancellationToken)
    {
        return await _collection.AsQueryable().ToListAsync(cancellationToken);
    }
    
    public async Task<DeckOutput> GetDeckById(string deckId, CancellationToken cancellationToken)
    {
        var pipeline = new[]
        {
            DeckServiceDocumentHelpers.MatchById(deckId)
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
}