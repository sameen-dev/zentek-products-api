using FluentAssertions;
using Products.Application.Dtos;
using Products.Application.Validation;
using Xunit;

namespace Products.UnitTests.Validation;

public class CreateProductRequestValidatorTests
{
    private readonly CreateProductRequestValidator _sut = new();

    [Fact]
    public void Validate_WithValidRequest_HasNoErrors()
    {
        var request = new CreateProductRequest("Widget", "SKU-123", "Red", 9.99m, "A widget");

        var result = _sut.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "SKU-1", "Red", 1.0, "Name")]
    [InlineData("Widget", "", "Red", 1.0, "Sku")]
    [InlineData("Widget", "SKU 1", "Red", 1.0, "Sku")]
    [InlineData("Widget", "SKU-1", "Purple", 1.0, "Colour")]
    [InlineData("Widget", "SKU-1", "Red", 0, "Price")]
    [InlineData("Widget", "SKU-1", "Red", -5, "Price")]
    public void Validate_WithInvalidField_ReturnsErrorForThatField(string name, string sku, string colour, decimal price, string expectedErrorProperty)
    {
        var request = new CreateProductRequest(name, sku, colour, price, null);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == expectedErrorProperty);
    }

    [Theory]
    [InlineData("black")]
    [InlineData("BLACK")]
    [InlineData("Black")]
    public void Validate_ColourIsCaseInsensitive(string colour)
    {
        var request = new CreateProductRequest("Widget", "SKU-1", colour, 1.0m, null);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeTrue();
    }
}
