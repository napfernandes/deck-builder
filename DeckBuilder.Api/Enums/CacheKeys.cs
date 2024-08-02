namespace DeckBuilder.Api.Enums;

public static class CacheKeys
{
    public const string DecksList = "decks_list";
    public const string UsersList = "users_list";
    public const string CountCards = "cards_count";
    public static readonly Func<string, string> DeckById = deckId => $"deck_byId_{deckId}";
    public static readonly Func<string, string> CardById = cardId => $"card_byId_{cardId}";
    public static readonly Func<string, string> GetUserById = userId => $"user_byId_{userId}";
    public static readonly Func<string, string> GetUserByEmail = email => $"user_byEmail_{email}";
    public static readonly Func<string, string> CardsBySet = (setCode) => $"cards_bySet_{setCode}";
    public static readonly Func<string, string> CardsSearchByQuery = query => $"cards_searchByQuery_{query}";
    public static readonly Func<string, string, string> CardBySetAndCode = (setCode, code) => $"card_bySetAndCode_{setCode}_{code}";
}