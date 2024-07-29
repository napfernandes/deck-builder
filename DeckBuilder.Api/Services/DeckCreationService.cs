using DeckBuilder.Api.Enums;
using DeckBuilder.Api.Exceptions;
using DeckBuilder.Api.Models;
using DeckBuilder.Api.ViewModels;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DeckBuilder.Api.Services;

public class DeckCreationService(IMongoDatabase database)
{
    private readonly IMongoCollection<Deck> _collection = database.GetCollection<Deck>(Collections.Decks);

    private void ValidateDeckCreation(CreateDeckInput input)
    {
        const int minimumNumberOfCards = 2;
        const int maxQuantityOfSameCard = 4;
        
        if (input.Cards.Count() < minimumNumberOfCards)
            throw KnownException.MinimumNumberOfCardsInDeck(minimumNumberOfCards);

        var cardWithNoQuantity = input.Cards.FirstOrDefault(c => c.Quantity == 0);
        if (cardWithNoQuantity is not null)
            throw KnownException.NoQuantityForDeckCard(cardWithNoQuantity.CardId);
            
        var numberOfCardsExceedingAmount = input.Cards.Count(c => c.Quantity > maxQuantityOfSameCard);
        if (numberOfCardsExceedingAmount > 0)
            throw KnownException.NumberOfCardsExceedingAmount(numberOfCardsExceedingAmount, maxQuantityOfSameCard);
    }
    
    public async Task<string> CreateDeck(CreateDeckInput input, CancellationToken cancellationToken)
    {
        ValidateDeckCreation(input);
        
        var newDeck = new Deck
        {
            Title = input.Title,
            Cards = input.Cards,
            CreatedBy = ObjectId.GenerateNewId().ToString(),
            CreatedAt = DateTime.UtcNow,
            Description = input.Description
        };

        await _collection.InsertOneAsync(newDeck, null, cancellationToken);

        return newDeck.Id;
    }
}