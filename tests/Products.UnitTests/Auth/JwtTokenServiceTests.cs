using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Products.Infrastructure.Auth;
using Products.Infrastructure.Options;
using Xunit;

namespace Products.UnitTests.Auth;

public class JwtTokenServiceTests
{
    private const string Username = "demo";
    private const string Password = "Demo123!";

    private readonly JwtTokenService _sut;

    public JwtTokenServiceTests()
    {
        var passwordHash = new PasswordHasher<object>().HashPassword(new object(), Password);

        var jwtOptions = Options.Create(new JwtOptions
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            SigningKey = "unit-test-signing-key-that-is-sufficiently-long",
            ExpiryMinutes = 30
        });

        var demoUserOptions = Options.Create(new DemoUserOptions
        {
            Username = Username,
            PasswordHash = passwordHash
        });

        _sut = new JwtTokenService(jwtOptions, demoUserOptions);
    }

    [Fact]
    public void IssueToken_WithCorrectCredentials_ReturnsTokenWithExpectedClaimsAndExpiry()
    {
        var before = DateTime.UtcNow;

        var token = _sut.IssueToken(Username, Password);

        token.Should().NotBeNull();
        token!.ExpiresAtUtc.Should().BeCloseTo(before.AddMinutes(30), TimeSpan.FromSeconds(5));

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token.AccessToken);
        jwt.Issuer.Should().Be("test-issuer");
        jwt.Audiences.Should().Contain("test-audience");
        jwt.Subject.Should().Be(Username);
    }

    [Theory]
    [InlineData("demo", "WrongPassword1!")]
    [InlineData("not-demo", "Demo123!")]
    public void IssueToken_WithIncorrectCredentials_ReturnsNull(string username, string password)
    {
        var token = _sut.IssueToken(username, password);

        token.Should().BeNull();
    }

    [Fact]
    public void IssueToken_UsernameComparisonIsCaseInsensitive()
    {
        var token = _sut.IssueToken("DEMO", Password);

        token.Should().NotBeNull();
    }
}
