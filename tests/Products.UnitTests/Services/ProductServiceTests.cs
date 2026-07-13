using FluentAssertions;
using Moq;
using Products.Application.Abstractions;
using Products.Application.Dtos;
using Products.Application.Exceptions;
using Products.Application.Services;
using Products.Domain.Entities;
using Products.Domain.Enums;
using Xunit;

namespace Products.UnitTests.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repository = new();
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _sut = new ProductService(_repository.Object);
    }

    [Fact]
    public async Task CreateAsync_WithNewSku_PersistsProductAndReturnsMappedResponse()
    {
        _repository.Setup(r => r.SkuExistsAsync("SKU-1", It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var request = new CreateProductRequest("Widget", "SKU-1", "Red", 12.50m, "A widget");

        var result = await _sut.CreateAsync(request, CancellationToken.None);

        result.Name.Should().Be("Widget");
        result.Sku.Should().Be("SKU-1");
        result.Colour.Should().Be("Red");
        result.Price.Should().Be(12.50m);
        _repository.Verify(r => r.AddAsync(It.Is<Product>(p => p.Sku == "SKU-1" && p.Colour == ProductColour.Red), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateSku_ThrowsDuplicateSkuException()
    {
        _repository.Setup(r => r.SkuExistsAsync("SKU-1", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var request = new CreateProductRequest("Widget", "SKU-1", "Red", 12.50m, null);

        var act = () => _sut.CreateAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<DuplicateSkuException>();
        _repository.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAllAsync_WithNoColourFilter_ReturnsAllMappedProducts()
    {
        var products = new List<Product>
        {
            new("Widget", "SKU-1", ProductColour.Red, 10m, null),
            new("Gadget", "SKU-2", ProductColour.Blue, 20m, null)
        };
        _repository.Setup(r => r.GetAllAsync(null, It.IsAny<CancellationToken>())).ReturnsAsync(products);

        var result = await _sut.GetAllAsync(null, CancellationToken.None);

        result.Should().HaveCount(2);
        result.Select(p => p.Sku).Should().BeEquivalentTo("SKU-1", "SKU-2");
    }

    [Fact]
    public async Task GetAllAsync_WithColourFilter_PassesFilterThroughToRepository()
    {
        var redProducts = new List<Product> { new("Widget", "SKU-1", ProductColour.Red, 10m, null) };
        _repository.Setup(r => r.GetAllAsync(ProductColour.Red, It.IsAny<CancellationToken>())).ReturnsAsync(redProducts);

        var result = await _sut.GetAllAsync(ProductColour.Red, CancellationToken.None);

        result.Should().ContainSingle().Which.Colour.Should().Be("Red");
        _repository.Verify(r => r.GetAllAsync(ProductColour.Red, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductMissing_ReturnsNull()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
    }
}
