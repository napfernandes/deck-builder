using DeckBuilder.Api.Enums;

namespace DeckBuilder.Api.Exceptions;

public class KnownException(string message, string code, Exception? exception = null)
    : Exception(message, exception)
{
    public string Code => code;

    public static KnownException ImportFileNotFound(string filePath, Exception? exception = null)
    {
        return new KnownException($"File not found on path \"{filePath}\".", ErrorCodes.ImportFileNotFound, exception);
    }

    public static KnownException CardsAlreadyImported(Exception? exception = null)
    {
        return new KnownException("Cards were already imported.", ErrorCodes.CardsAlreadyImported, exception);
    }
    
    public static KnownException MinimumNumberOfCardsInDeck(int minimumNumberOfCards, Exception? exception = null)
    {
        return new KnownException(
            $"Your deck should contain at least {minimumNumberOfCards} cards.",
            ErrorCodes.MinimumNumberOfCardsInDeck,
            exception);
    }
    
    public static KnownException NoQuantityForDeckCard(string cardId, Exception? exception = null)
    {
        return new KnownException(
            $"You must specify a quantity of cards for {cardId}.",
            ErrorCodes.NoQuantityForDeckCard,
            exception);
    }
    
    public static KnownException NumberOfCardsExceedingAmount(int numberOfCards, int maximumAmount, Exception? exception = null)
    {
        return new KnownException(
            $"There are {numberOfCards} card(s) exceeding the maximum amount of {maximumAmount} copies.",
            ErrorCodes.NumberOfCardsExceedingAmount,
            exception);
    }

    public static KnownException UserAlreadyExists(string email, Exception? exception = null)
    {
        return new KnownException(
            $"User already exists ({email})",
            ErrorCodes.UserAlreadyExists,
            exception);
    }

    public static KnownException InvalidCredentials(Exception? exception = null)
    {
        return new KnownException("Invalid credentials.", ErrorCodes.InvalidCredentials, exception);
    }
    
    public static KnownException DeckNotFound(string deckId, Exception? exception = null)
    {
        return new KnownException($"Deck not found ({deckId}).", ErrorCodes.InvalidCredentials, exception);
    }
}