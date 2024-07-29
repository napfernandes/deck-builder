using DeckBuilder.Api.ViewModels;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DeckBuilder.Api.Models;

public class Deck
{
    [BsonId]
    [BsonElement("id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; init; } = string.Empty;

    [BsonElement("title")]
    public string Title { get; init; } = string.Empty;
    
    [BsonElement("description")]
    public string Description { get; init; } = string.Empty;

    [BsonElement("createdBy")]
    public string CreatedBy { get; init; } = string.Empty;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; init; }

    [BsonElement("cards")]
    public IEnumerable<DeckCard> Cards { get; init; } = [];
}