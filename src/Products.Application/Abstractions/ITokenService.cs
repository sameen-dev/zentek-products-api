using Products.Application.Dtos;

namespace Products.Application.Abstractions;

public interface ITokenService
{
    /// <summary>
    /// Validates the supplied demo credentials and, if valid, issues a signed JWT.
    /// Returns null when the credentials do not match a known user.
    /// </summary>
    TokenResponse? IssueToken(string username, string password);
}
