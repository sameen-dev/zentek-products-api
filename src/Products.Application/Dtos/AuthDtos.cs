namespace Products.Application.Dtos;

public sealed record TokenRequest(string Username, string Password);

public sealed record TokenResponse(string AccessToken, DateTime ExpiresAtUtc);
