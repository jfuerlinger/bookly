namespace Bookly.Api.Integration.Tests.Infrastructure;

/// <summary>
/// xUnit collection definition that shares a single PostgreSQL container
/// across all tests in this collection.
/// </summary>
[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<PostgresContainer>
{
    public const string Name = "Integration";
}
