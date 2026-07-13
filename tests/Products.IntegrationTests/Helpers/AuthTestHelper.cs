using System.Net.Http.Headers;
using System.Net.Http.Json;
using Products.Application.Dtos;

namespace Products.IntegrationTests.Helpers;

public static class AuthTestHelper
{
    public const string DemoUsername = "demo";
    public const string DemoPassword = "Demo123!";

    public static async Task<string> GetAccessTokenAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/token", new TokenRequest(DemoUsername, DemoPassword));
        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadFromJsonAsync<TokenResponse>();
        return token!.AccessToken;
    }

    public static async Task<HttpClient> CreateAuthenticatedClientAsync(this HttpClient client)
    {
        var accessToken = await GetAccessTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }
}
