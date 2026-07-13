using System.Net;
using FluentAssertions;
using Products.IntegrationTests.Fixtures;
using Xunit;

namespace Products.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public class HealthEndpointTests(ProductsApiFactory factory)
{
    [Fact]
    public async Task GetHealth_WithoutAuthentication_ReturnsOk()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Healthy");
    }
}
