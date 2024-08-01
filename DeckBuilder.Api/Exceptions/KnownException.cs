using DeckBuilder.Api.Enums;

namespace DeckBuilder.Api.Exceptions;

public class KnownException(string message, int statusCode, Exception? exception = null)
    : Exception(message, exception)
{
    public int StatusCode => statusCode;

    public static KnownException ImportFileNotFound(string filePath, Exception? exception = null)
    {
        return new KnownException($"File not found on path \"{filePath}\".", StatusCodes.Status404NotFound, exception);
    }

    public static KnownException CardsAlreadyImported(Exception? exception = null)
    {
        return new KnownException("Cards were already imported.", StatusCodes.Status400BadRequest, exception);
    }
    
    public static KnownException MinimumNumberOfCardsInDeck(int minimumNumberOfCards, Exception? exception = null)
    {
        return new KnownException(
            $"Your deck should contain at least {minimumNumberOfCards} cards.",
            StatusCodes.Status422UnprocessableEntity,
            exception);
    }
    
    public static KnownException NoQuantityForDeckCard(string cardId, Exception? exception = null)
    {
        return new KnownException(
            $"You must specify a quantity of cards for {cardId}.",
            StatusCodes.Status422UnprocessableEntity,
            exception);
    }
    
    public static KnownException NumberOfCardsExceedingAmount(int numberOfCards, int maximumAmount, Exception? exception = null)
    {
        return new KnownException(
            $"There are {numberOfCards} card(s) exceeding the maximum amount of {maximumAmount} copies.",
            StatusCodes.Status422UnprocessableEntity,
            exception);
    }

    public static KnownException UserAlreadyExists(string email, Exception? exception = null)
    {
        return new KnownException(
            $"User already exists ({email})",
            StatusCodes.Status409Conflict,
            exception);
    }

    public static KnownException InvalidCredentials(Exception? exception = null)
    {
        return new KnownException("Invalid credentials.", StatusCodes.Status400BadRequest, exception);
    }
    
    public static KnownException DeckNotFound(string deckId, Exception? exception = null)
    {
        return new KnownException($"Deck not found ({deckId}).", StatusCodes.Status404NotFound, exception);
    }
    
    public static KnownException CardNotFoundById(string cardId, Exception? exception = null)
    {
        return new KnownException($"Card not found ({cardId}).", StatusCodes.Status404NotFound, exception);
    }
    
    public static KnownException CardNotFoundBySetAndCode(string setCode, string code, Exception? exception = null)
    {
        return new KnownException($"Card not found ({setCode}, {code}).", StatusCodes.Status404NotFound, exception);
    }
}