using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Products.Infrastructure.Persistence;
using Xunit;

namespace Products.IntegrationTests.Fixtures;

/// <summary>
/// Boots the full API pipeline (WebApplicationFactory) against a uniquely-named LocalDB
/// database. The DbContextOptions registered by the app's own Program.cs are swapped out
/// for ones pointing at this fixture's database (a plain configuration override is not
/// reliable here: Program.cs reads the connection string eagerly while building services,
/// which happens before WebApplicationFactory's configuration hooks can take effect).
/// The database is dropped when the fixture is disposed so test runs don't accumulate state.
/// </summary>
public sealed class ProductsApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _databaseName = $"ProductsDb_IntegrationTests_{Guid.NewGuid():N}";

    public string ConnectionString =>
        $"Server=(localdb)\\MSSQLLocalDB;Database={_databaseName};Trusted_Connection=True;TrustServerCertificate=True;";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ProductsDbContext>>();
            services.AddDbContext<ProductsDbContext>(options => options.UseSqlServer(ConnectionString));
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ProductsDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ProductsDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
    }
}
