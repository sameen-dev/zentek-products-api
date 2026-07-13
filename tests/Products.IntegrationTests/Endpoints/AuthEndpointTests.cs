using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Products.Application.Dtos;
using Products.IntegrationTests.Fixtures;
using Products.IntegrationTests.Helpers;
using Xunit;

namespace Products.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public class AuthEndpointTests(ProductsApiFactory factory)
{
    [Fact]
    public async Task PostToken_WithValidCredentials_ReturnsJwt()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/token", new TokenRequest(AuthTestHelper.DemoUsername, AuthTestHelper.DemoPassword));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var token = await response.Content.ReadFromJsonAsync<TokenResponse>();
        token!.AccessToken.Should().NotBeNullOrWhiteSpace();
        token.ExpiresAtUtc.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task PostToken_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/token", new TokenRequest("demo", "wrong-password"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
