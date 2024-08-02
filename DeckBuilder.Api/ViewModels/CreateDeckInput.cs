using DeckBuilder.Api.Models;

namespace DeckBuilder.Api.ViewModels;

public record CreateDeckInput(string Title, string Description, IEnumerable<DeckCard> Cards, string? CreatedBy = null);