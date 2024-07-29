namespace DeckBuilder.Api.ViewModels;

public record DeckOutput
{
    public string Id { get; init; } = string.Empty;  
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string CreatedBy { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    
    public IEnumerable<CardOutput> Cards { get; init; } = [];
}