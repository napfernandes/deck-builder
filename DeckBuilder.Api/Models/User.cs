using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DeckBuilder.Api.Models;

public class User
{
    [BsonId]
    [BsonElement("id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; init; } = string.Empty;
    
    [BsonElement("firstName")]
    public string FirstName { get; init; } = string.Empty;
    
    [BsonElement("lastName")]
    public string LastName { get; init; } = string.Empty;

    [BsonIgnore] public string FullName => $"{FirstName} {LastName}".Trim();
    
    [BsonElement("email")]
    public string Email { get; init; } = string.Empty;
    
    [BsonElement("password")]
    public string Password { get; init; } = string.Empty;
    
    [BsonElement("salt")]
    public string Salt { get; init; } = string.Empty;
    
    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; init; }
    
    [BsonElement("updatedAt")]
    public DateTime? UpdatedAt { get; init; }

    [BsonElement("decks")]
    public IEnumerable<string> Decks { get; init; } = [];
}