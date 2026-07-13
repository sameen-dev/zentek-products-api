using Xunit;

namespace Products.IntegrationTests.Fixtures;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<ProductsApiFactory>
{
    public const string Name = "Products API integration tests";
}
