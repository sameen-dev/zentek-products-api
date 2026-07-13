using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Products.Application.Dtos;
using Products.IntegrationTests.Fixtures;
using Products.IntegrationTests.Helpers;
using Xunit;

namespace Products.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public class ProductEndpointsTests(ProductsApiFactory factory)
{
    private static string UniqueSku() => $"SKU-{Guid.NewGuid():N}"[..12].ToUpperInvariant();

    [Fact]
    public async Task GetProducts_WithoutAuthentication_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/products");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostProduct_WithoutAuthentication_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/products", new CreateProductRequest("Widget", UniqueSku(), "Red", 9.99m, null));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateThenGetById_RoundTripsTheSameProduct()
    {
        var client = await factory.CreateClient().CreateAuthenticatedClientAsync();
        var sku = UniqueSku();

        var createResponse = await client.PostAsJsonAsync("/api/products", new CreateProductRequest("Widget", sku, "Green", 15.00m, "A green widget"));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        createResponse.Headers.Location.Should().NotBeNull();

        var created = await createResponse.Content.ReadFromJsonAsync<ProductResponse>();
        created!.Sku.Should().Be(sku);

        var getResponse = await client.GetAsync(createResponse.Headers.Location);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await getResponse.Content.ReadFromJsonAsync<ProductResponse>();
        fetched!.Id.Should().Be(created.Id);
        fetched.Colour.Should().Be("Green");
    }

    [Fact]
    public async Task GetProducts_ReturnsAllCreatedProducts()
    {
        var client = await factory.CreateClient().CreateAuthenticatedClientAsync();
        var sku = UniqueSku();

        var createResponse = await client.PostAsJsonAsync("/api/products", new CreateProductRequest("ListMe", sku, "Blue", 5.00m, null));
        createResponse.EnsureSuccessStatusCode();

        var listResponse = await client.GetAsync("/api/products");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var products = await listResponse.Content.ReadFromJsonAsync<List<ProductResponse>>();
        products.Should().Contain(p => p.Sku == sku);
    }

    [Fact]
    public async Task GetProductsByColour_ReturnsOnlyMatchingColour()
    {
        var client = await factory.CreateClient().CreateAuthenticatedClientAsync();
        var yellowSku = UniqueSku();
        var silverSku = UniqueSku();

        await client.PostAsJsonAsync("/api/products", new CreateProductRequest("YellowThing", yellowSku, "Yellow", 3.00m, null));
        await client.PostAsJsonAsync("/api/products", new CreateProductRequest("SilverThing", silverSku, "Silver", 3.00m, null));

        var response = await client.GetAsync("/api/products?colour=Yellow");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var products = await response.Content.ReadFromJsonAsync<List<ProductResponse>>();
        products.Should().Contain(p => p.Sku == yellowSku);
        products.Should().NotContain(p => p.Sku == silverSku);
        products.Should().OnlyContain(p => p.Colour == "Yellow");
    }

    [Fact]
    public async Task GetProducts_WithInvalidColour_ReturnsBadRequest()
    {
        var client = await factory.CreateClient().CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/products?colour=NotAColour");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostProduct_WithInvalidPayload_ReturnsValidationProblemDetails()
    {
        var client = await factory.CreateClient().CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/products", new CreateProductRequest("", "", "NotAColour", -1m, null));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Name").And.Contain("Sku").And.Contain("Colour").And.Contain("Price");
    }

    [Fact]
    public async Task PostProduct_WithDuplicateSku_ReturnsConflict()
    {
        var client = await factory.CreateClient().CreateAuthenticatedClientAsync();
        var sku = UniqueSku();

        var first = await client.PostAsJsonAsync("/api/products", new CreateProductRequest("Widget", sku, "Black", 1.00m, null));
        first.EnsureSuccessStatusCode();

        var second = await client.PostAsJsonAsync("/api/products", new CreateProductRequest("Widget Again", sku, "White", 2.00m, null));

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetProductById_WhenNotFound_ReturnsNotFound()
    {
        var client = await factory.CreateClient().CreateAuthenticatedClientAsync();

        var response = await client.GetAsync($"/api/products/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
