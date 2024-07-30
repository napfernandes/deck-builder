using DeckBuilder.Api.Enums;
using DeckBuilder.Api.Exceptions;
using DeckBuilder.Api.Models;
using DeckBuilder.Api.ViewModels;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DeckBuilder.Api.Services;

public class DeckCreationService(IMongoDatabase database, CardService cardService)
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

        var cards = await cardService.GetCardDetailsByIds(input.Cards.Select(c => c.CardId), cancellationToken);
        
        var newDeck = new Deck
        {
            Title = input.Title,
            Cards = input.Cards.Select(card => new DeckCard
            {
                Notes = card.Notes,
                CardId = card.CardId,
                Quantity = card.Quantity,
                Details = cards.Single(c => c.Id == card.CardId).Attributes
            }),
            CreatedBy = ObjectId.GenerateNewId().ToString(),
            CreatedAt = DateTime.UtcNow,
            Description = input.Description
        };

        await _collection.InsertOneAsync(newDeck, null, cancellationToken);

        return newDeck.Id;
    }
}