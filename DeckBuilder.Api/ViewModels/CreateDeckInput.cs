using DeckBuilder.Api.Models;

namespace DeckBuilder.Api.ViewModels;

public record CreateDeckInput
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public IEnumerable<DeckCard> Cards { get; init; } = [];
}
