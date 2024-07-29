using MongoDB.Bson.Serialization.Attributes;

namespace DeckBuilder.Api.Models;

public record Attribute
{
    [BsonElement("key")] public string Key { get; init; } = string.Empty;

    [BsonElement("displayText")] public string DisplayText { get; init; } = string.Empty;

    [BsonElement("value")] public string? Value { get; init; } = string.Empty;
    [BsonElement("values")] public IEnumerable<string>? Values { get; init; } = [];

    [BsonElement("searchable")] public bool Searchable { get; init; }

    [BsonElement("visible")] public bool Visible { get; init; }

    [BsonElement("language")] public string Language { get; init; } = string.Empty;
}