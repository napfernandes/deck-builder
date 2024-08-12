using DeckBuilder.Api.Exceptions;
using DeckBuilder.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeckBuilder.Api.Routes;

public static class SetRoutes
{
    public static void RegisterSetEndpoints(this IEndpointRouteBuilder routes)
    {
        var sets = routes.MapGroup("/api/v1/sets");

        sets
            .MapGet("/{setCode}/cards", async (string setCode, CardService service, CancellationToken cancellationToken) =>
                await service.GetCardsBySet(setCode, cancellationToken))
            .WithName("getCardsBySetNameOrCode")
            .WithDescription("Get all cards by a set (either its name or code)+")
            .WithOpenApi();

        sets
            .MapGet("/{setCode}/cards/{code}", async (string setCode, string code, CardService service, CancellationToken cancellationToken) =>
                await service.GetCardBySetAndCode(setCode, code, cancellationToken))
            .WithName("getCardBySetAndCode")
            .WithDescription("Get a card by its set code and the card code itself")
            .WithOpenApi();
        
        sets
            .MapGet("/{setCode}/packs/generate", async (string setCode, CardService service, CancellationToken cancellationToken) =>
                await service.GenerateRandomPackForSet(setCode, cancellationToken))
            .RequireAuthorization()
            .WithName("generateRandomPackForSet")
            .WithDescription("Generate a random pack of cards from a given set")
            .WithOpenApi();
    }
}