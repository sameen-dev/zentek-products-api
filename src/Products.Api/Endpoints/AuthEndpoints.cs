using Products.Application.Abstractions;
using Products.Application.Dtos;

namespace Products.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/token", (TokenRequest request, ITokenService tokenService) =>
        {
            var token = tokenService.IssueToken(request.Username, request.Password);
            return token is null
                ? Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid username or password.")
                : Results.Ok(token);
        })
        .WithName("IssueToken")
        .AllowAnonymous()
        .Produces<TokenResponse>()
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
