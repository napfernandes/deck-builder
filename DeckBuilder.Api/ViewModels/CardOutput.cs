using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DeckBuilder.Api.ViewModels;

public record CardOutput
{
    [BsonElement("id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; init; } = string.Empty;
    
    [BsonElement("language")]
    public string Language { get; init; } = string.Empty;
    
    [BsonElement("attributes")]
    public Dictionary<string, object?> Attributes { get; init; } = new();
}