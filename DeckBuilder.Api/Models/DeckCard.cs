using MongoDB.Bson.Serialization.Attributes;

namespace DeckBuilder.Api.Models;

public class DeckCard
{
    [BsonElement("cardId")]
    public string CardId { get; init; } = string.Empty;
    
    [BsonElement("quantity")]
    public int Quantity { get; init; }
    
    [BsonElement("notes")]
    public string? Notes { get; init; } = string.Empty;

    [BsonElement("details")]
    public Dictionary<string, object?> Details { get; init; } = new();
}