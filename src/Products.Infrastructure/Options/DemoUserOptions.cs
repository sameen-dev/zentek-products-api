namespace Products.Infrastructure.Options;

/// <summary>
/// Backs the single demo account used to obtain a JWT from this sample API.
/// A real service would replace this with a proper identity provider / user store.
/// </summary>
public sealed class DemoUserOptions
{
    public const string SectionName = "DemoUser";

    public required string Username { get; set; }

    /// <summary>PBKDF2 hash produced by <see cref="Microsoft.AspNetCore.Identity.PasswordHasher{TUser}"/>.</summary>
    public required string PasswordHash { get; set; }
}
