using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DeckBuilder.Api.Models;

public class Game
{
    [BsonId]
    [BsonElement("id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; init; } = string.Empty;
    
    [BsonElement("name")]
    public string Name { get; init; } = string.Empty;
    
    [BsonElement("description")]
    public string Description { get; init; } = string.Empty;
    
    [BsonElement("numberOfCards")]
    public int NumberOfCards { get; init; }
}
