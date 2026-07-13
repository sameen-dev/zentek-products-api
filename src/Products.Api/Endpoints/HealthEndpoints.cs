namespace Products.Api.Endpoints;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestampUtc = DateTime.UtcNow }))
            .WithName("HealthCheck")
            .WithTags("Health")
            .AllowAnonymous()
            .Produces(StatusCodes.Status200OK);

        return app;
    }
}
