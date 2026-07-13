using Products.Application.Dtos;
using Products.Domain.Entities;

namespace Products.Application.Mapping;

public static class ProductMappingExtensions
{
    public static ProductResponse ToResponse(this Product product) => new(
        product.Id,
        product.Name,
        product.Sku,
        product.Colour.ToString(),
        product.Price,
        product.Description,
        product.CreatedUtc);
}
