using Products.Application.Abstractions;
using Products.Application.Dtos;
using Products.Application.Exceptions;
using Products.Application.Mapping;
using Products.Domain.Entities;
using Products.Domain.Enums;

namespace Products.Application.Services;

public sealed class ProductService(IProductRepository repository) : IProductService
{
    public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        if (await repository.SkuExistsAsync(request.Sku, cancellationToken))
        {
            throw new DuplicateSkuException(request.Sku);
        }

        // Colour is validated upstream (FluentValidation); TryParse here is a defensive fallback.
        ProductColourParser.TryParse(request.Colour, out var colour);

        var product = new Product(request.Name, request.Sku, colour, request.Price, request.Description);

        await repository.AddAsync(product, cancellationToken);

        return product.ToResponse();
    }

    public async Task<ProductResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(id, cancellationToken);
        return product?.ToResponse();
    }

    public async Task<IReadOnlyList<ProductResponse>> GetAllAsync(ProductColour? colour, CancellationToken cancellationToken)
    {
        var products = await repository.GetAllAsync(colour, cancellationToken);
        return products.Select(p => p.ToResponse()).ToList();
    }
}
