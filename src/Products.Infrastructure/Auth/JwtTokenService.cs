using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Products.Application.Abstractions;
using Products.Application.Dtos;
using Products.Infrastructure.Options;

namespace Products.Infrastructure.Auth;

public sealed class JwtTokenService(IOptions<JwtOptions> jwtOptions, IOptions<DemoUserOptions> demoUserOptions)
    : ITokenService
{
    private static readonly IPasswordHasher<object> PasswordHasher = new PasswordHasher<object>();

    public TokenResponse? IssueToken(string username, string password)
    {
        var demoUser = demoUserOptions.Value;

        if (!string.Equals(username, demoUser.Username, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var verification = PasswordHasher.VerifyHashedPassword(new object(), demoUser.PasswordHash, password);
        if (verification == PasswordVerificationResult.Failed)
        {
            return null;
        }

        var jwt = jwtOptions.Value;
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(jwt.ExpiryMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, demoUser.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwt.Issuer,
            audience: jwt.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        return new TokenResponse(accessToken, expiresAtUtc);
    }
}
