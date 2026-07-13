using Microsoft.EntityFrameworkCore;
using Products.Application.Abstractions;
using Products.Domain.Entities;
using Products.Domain.Enums;
using Products.Infrastructure.Persistence;

namespace Products.Infrastructure.Repositories;

public sealed class ProductRepository(ProductsDbContext dbContext) : IProductRepository
{
    public async Task AddAsync(Product product, CancellationToken cancellationToken)
    {
        await dbContext.Products.AddAsync(product, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Product>> GetAllAsync(ProductColour? colour, CancellationToken cancellationToken)
    {
        var query = dbContext.Products.AsNoTracking();

        if (colour is not null)
        {
            query = query.Where(p => p.Colour == colour);
        }

        return await query
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken)
    {
        var normalized = sku.Trim().ToUpperInvariant();
        return await dbContext.Products.AsNoTracking().AnyAsync(p => p.Sku == normalized, cancellationToken);
    }
}
