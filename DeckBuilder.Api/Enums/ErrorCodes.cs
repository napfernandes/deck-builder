namespace DeckBuilder.Api.Enums;

public class ErrorCodes
{
    public const string DeckNotFound = "deck_not_found";
    public const string UserAlreadyExists = "user_already_exists";
    public const string InvalidCredentials = "invalid_credentials";
    public const string ImportFileNotFound = "import_file_not_found";
    public const string CardsAlreadyImported = "cards_already_imported";
    public const string NoQuantityForDeckCard = "no_quantity_for_deck_card";
    public const string MinimumNumberOfCardsInDeck = "minimum_number_of_cards_in_deck";
    public const string NumberOfCardsExceedingAmount = "number_of_cards_exceeding_amount";
}
