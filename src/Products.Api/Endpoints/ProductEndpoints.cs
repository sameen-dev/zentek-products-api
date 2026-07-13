using Products.Api.Filters;
using Products.Application.Dtos;
using Products.Application.Mapping;
using Products.Application.Services;

namespace Products.Api.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products")
            .RequireAuthorization()
            .RequireRateLimiting("products");

        group.MapPost("/", async (CreateProductRequest request, IProductService productService, CancellationToken cancellationToken) =>
        {
            var product = await productService.CreateAsync(request, cancellationToken);
            return Results.Created($"/api/products/{product.Id}", product);
        })
        .WithName("CreateProduct")
        .AddEndpointFilter<ValidationFilter<CreateProductRequest>>()
        .Produces<ProductResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem();

        group.MapGet("/", async (string? colour, IProductService productService, CancellationToken cancellationToken) =>
        {
            if (colour is not null && !ProductColourParser.TryParse(colour, out _))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["colour"] = [$"'{colour}' is not a valid colour. Valid values: {ProductColourParser.ValidValues}."]
                });
            }

            ProductColourParser.TryParse(colour, out var parsedColour);
            var products = await productService.GetAllAsync(colour is null ? null : parsedColour, cancellationToken);
            return Results.Ok(products);
        })
        .WithName("GetProducts")
        .Produces<IReadOnlyList<ProductResponse>>()
        .ProducesValidationProblem();

        group.MapGet("/{id:guid}", async (Guid id, IProductService productService, CancellationToken cancellationToken) =>
        {
            var product = await productService.GetByIdAsync(id, cancellationToken);
            return product is null ? Results.NotFound() : Results.Ok(product);
        })
        .WithName("GetProductById")
        .Produces<ProductResponse>()
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
