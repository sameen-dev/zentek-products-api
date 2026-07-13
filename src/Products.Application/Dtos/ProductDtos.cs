namespace Products.Application.Dtos;

public sealed record CreateProductRequest(
    string Name,
    string Sku,
    string Colour,
    decimal Price,
    string? Description);

public sealed record ProductResponse(
    Guid Id,
    string Name,
    string Sku,
    string Colour,
    decimal Price,
    string? Description,
    DateTime CreatedUtc);
