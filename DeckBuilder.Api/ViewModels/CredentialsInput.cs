namespace DeckBuilder.Api.ViewModels;

public record CredentialsInput
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}