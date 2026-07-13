using Microsoft.EntityFrameworkCore;
using Products.Domain.Entities;
using Products.Domain.Enums;

namespace Products.Infrastructure.Persistence;

public class ProductsDbContext(DbContextOptions<ProductsDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(builder =>
        {
            builder.ToTable("Products");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Sku)
                .IsRequired()
                .HasMaxLength(64);

            builder.HasIndex(p => p.Sku)
                .IsUnique();

            builder.Property(p => p.Colour)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(32);

            builder.Property(p => p.Price)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.Description)
                .HasMaxLength(1000);

            builder.Property(p => p.CreatedUtc)
                .IsRequired();
        });
    }
}
