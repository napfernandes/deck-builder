using System.Text.Json.Serialization;

namespace DeckBuilder.Api.ViewModels;

public record CreateDeckOutput([property: JsonPropertyName("id")] string Id);