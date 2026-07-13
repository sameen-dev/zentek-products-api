using Products.Application.Dtos;
using Products.Domain.Enums;

namespace Products.Application.Services;

public interface IProductService
{
    Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken);

    Task<ProductResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<ProductResponse>> GetAllAsync(ProductColour? colour, CancellationToken cancellationToken);
}
