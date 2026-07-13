using Products.Domain.Enums;

namespace Products.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Sku { get; private set; } = null!;
    public ProductColour Colour { get; private set; }
    public decimal Price { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedUtc { get; private set; }

    private Product()
    {
        // Required by EF Core for materialization.
    }

    public Product(string name, string sku, ProductColour colour, decimal price, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Product name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new ArgumentException("Product SKU is required.", nameof(sku));
        }

        if (price <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), price, "Product price must be greater than zero.");
        }

        Id = Guid.NewGuid();
        Name = name.Trim();
        Sku = sku.Trim().ToUpperInvariant();
        Colour = colour;
        Price = price;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        CreatedUtc = DateTime.UtcNow;
    }
}
