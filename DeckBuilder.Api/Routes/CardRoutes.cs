using DeckBuilder.Api.Exceptions;
using DeckBuilder.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeckBuilder.Api.Routes;

public static class CardRoutes
{
    public static void RegisterCardEndpoints(this IEndpointRouteBuilder routes)
    {
        var users = routes.MapGroup("/api/v1/cards");

        users
            .MapGet("/", async ([FromQuery] string? query, CardService service, CancellationToken cancellationToken) =>
                await service.SearchCards(query, cancellationToken))
            .WithName("getAllCards")
            .WithDescription("Get all cards")
            .WithOpenApi();

        users.MapGet("/count",
                async (CardService service, CancellationToken cancellationToken) =>
                    await service.CountNumberOfCards(cancellationToken))
            .WithName("countCards")
            .WithDescription("Counting all cards")
            .WithOpenApi();

        users
            .MapGet("/{cardId}", async (string cardId, CardService service, CancellationToken cancellationToken) =>
                await service.GetCardById(cardId, cancellationToken))
            .WithName("getCardById")
            .WithDescription("Get a card by its ID")
            .WithOpenApi();

        users
            .MapPost("/import", async (CardService cardService, ImportService importService, CancellationToken cancellationToken) =>
            {
                var numberOfCards = await cardService.CountNumberOfCards(cancellationToken);

                if (numberOfCards > 0)
                    throw KnownException.CardsAlreadyImported();

                await importService.ImportCardsFromAssets(cancellationToken);
            })
            .WithName("importCards")
            .WithDescription("Import cards data into the system")
            .WithOpenApi();

        users
            .MapDelete("/import",
                async (ImportService service, CancellationToken cancellationToken) =>
                {
                    await service.DeleteImportedData(cancellationToken);
                })
            .WithName("deleteImportedCards")
            .WithDescription("Delete all cards from cards collection")
            .WithOpenApi();
    }
}