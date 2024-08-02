using DeckBuilder.Api.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DeckBuilder.Api.ViewModels;

public record DeckOutput
{
    [BsonElement("id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; init; } = string.Empty;
    
    [BsonElement("title")]
    public string Title { get; init; } = string.Empty;

    [BsonElement("gameId")]
    public string GameId { get; init; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; init; } = string.Empty;
    
    [BsonElement("createdBy")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string CreatedBy { get; init; } = string.Empty;

    [BsonElement("createdByUser")]
    public DeckUserOutput? CreatedByUser { get; init; } = null!;
    
    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; init; }
    
    [BsonElement("cards")]
    public IEnumerable<DeckCard> Cards { get; init; } = [];
}

public record DeckUserOutput
{
    [BsonId]
    [BsonElement("id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; init; } = string.Empty;
    
    [BsonElement("firstName")]
    public string FirstName { get; init; } = string.Empty;
    
    [BsonElement("lastName")]
    public string LastName { get; init; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; init; } = string.Empty;
    
    [BsonIgnore]
    public string FullName => $"{FirstName} {LastName}".Trim();
}