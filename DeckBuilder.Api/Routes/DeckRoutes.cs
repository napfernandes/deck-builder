using DeckBuilder.Api.Services;
using DeckBuilder.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DeckBuilder.Api.Routes;

public static class DeckRoutes
{
    public static void RegisterDeckEndpoints(this IEndpointRouteBuilder routes)
    {
        var decks = routes.MapGroup("/api/v1/decks");

        decks
            .MapGet("", async (DeckService service, CancellationToken cancellationToken)
                => await service.SearchDecks(cancellationToken))
            .WithName("getAllDecks")
            .WithDescription("Get all decks registered in the platform")
            .WithOpenApi();
        
        decks.MapPost("", async ([FromBody] CreateDeckInput input, DeckCreationService service, CancellationToken cancellationToken)
                => await service.CreateDeck(input, cancellationToken))
            .WithName("createDeck")
            .WithDescription("Creates a deck for a given user after validation of an input")
            .WithOpenApi();
    }
}