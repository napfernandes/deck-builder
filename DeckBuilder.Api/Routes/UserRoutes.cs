using DeckBuilder.Api.Services;
using DeckBuilder.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DeckBuilder.Api.Routes;

public static class UserRoutes
{
    public static void RegisterUserEndpoints(this IEndpointRouteBuilder routes)
    {
        var users = routes.MapGroup("/api/v1/users");

        users
            .MapPost("", async ([FromBody] CreateUserInput input, UserService service, CancellationToken ct)
                => await service.CreateUser(input, ct))
            .WithName("createUser")
            .WithDescription("Create an user")
            .WithOpenApi();

        users
            .MapPost("login", async ([FromBody] CredentialsInput input, UserService service, CancellationToken ct)
                => await service.LoginWithCredentails(input, ct))
            .WithName("login")
            .WithDescription("Login with user credentials")
            .WithOpenApi();
    }
}