using DeckBuilder.Api.ViewModels;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DeckBuilder.Api.Models;

public record Card
{
    [BsonId]
    [BsonElement("id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; init; } = string.Empty;

    [BsonElement("language")]
    public string Language { get; init; } = string.Empty;

    [BsonElement("attributes")]
    public List<Attribute> Attributes { get; init; } = [];

    public CardOutput ToCardOutput()
    {
        var output = new CardOutput
        {
            Id = Id,
            Language = Language,
            Attributes = Attributes
                .Select(a => new KeyValuePair<string, object?>(a.Key, a.Values?.Count() > 0 ? a.Values.ToArray() : a.Value))
                .ToDictionary()
        };
        return output;
    }
}