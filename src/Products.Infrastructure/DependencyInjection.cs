using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Products.Application.Abstractions;
using Products.Infrastructure.Auth;
using Products.Infrastructure.Options;
using Products.Infrastructure.Persistence;
using Products.Infrastructure.Repositories;

namespace Products.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ProductsDb")
            ?? throw new InvalidOperationException("Missing required connection string 'ProductsDb'.");

        services.AddDbContext<ProductsDbContext>(options => options.UseSqlServer(connectionString));

        services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<DemoUserOptions>()
            .Bind(configuration.GetSection(DemoUserOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        return services;
    }
}
