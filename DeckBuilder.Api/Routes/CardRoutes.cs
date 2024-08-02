using DeckBuilder.Api.Exceptions;
using DeckBuilder.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeckBuilder.Api.Routes;

public static class CardRoutes
{
    public static void RegisterCardEndpoints(this IEndpointRouteBuilder routes)
    {
        var cards = routes.MapGroup("/api/v1/cards");

        cards
            .MapGet("/", async ([FromQuery] string? query, CardService service, CancellationToken cancellationToken) =>
                await service.SearchCards(query, cancellationToken))
            .WithName("getAllCards")
            .WithDescription("Get all cards")
            .WithOpenApi();

        cards
            .MapGet("/count",
                async (CardService service, CancellationToken cancellationToken) =>
                    await service.CountNumberOfCards(cancellationToken))
            .WithName("countCards")
            .WithDescription("Counting all cards")
            .WithOpenApi();

        cards
            .MapGet("/{cardId}", async (string cardId, CardService service, CancellationToken cancellationToken) =>
                await service.GetCardById(cardId, cancellationToken))
            .WithName("getCardById")
            .WithDescription("Get a card by its ID")
            .WithOpenApi();

        cards
            .MapPost("/import", async (ImportService importService, CancellationToken cancellationToken) =>
                await importService.ImportCardsFromAssets(cancellationToken))
            .WithName("importCards")
            .WithDescription("Import cards data into the system")
            .WithOpenApi();

        cards
            .MapDelete("/import", async (ImportService service, CancellationToken cancellationToken) =>
                    await service.DeleteImportedData(cancellationToken))
            .WithName("deleteImportedCards")
            .WithDescription("Delete all cards from cards collection")
            .WithOpenApi();
    }
}