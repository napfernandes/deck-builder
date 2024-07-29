using DeckBuilder.Api.Enums;
using DeckBuilder.Api.Exceptions;
using DeckBuilder.Api.Models;
using DeckBuilder.Api.ViewModels;
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
        var deck = await _collection
            .AsQueryable()
            .FirstOrDefaultAsync(d => Equals(d.Id, deckId), cancellationToken);
        
        if (deck is null)
            throw KnownException.DeckNotFound(deckId);

        return new DeckOutput();
    }
}