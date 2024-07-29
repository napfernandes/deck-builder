namespace DeckBuilder.Api.ViewModels;

public class CreateUserInput
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public IEnumerable<string>? Decks { get; init; } = [];
}
