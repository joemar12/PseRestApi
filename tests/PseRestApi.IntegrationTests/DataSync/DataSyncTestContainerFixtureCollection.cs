using Xunit;

namespace PseRestApi.IntegrationTests.DataSync;

/// <summary>
/// xUnit collection that ensures the shared PostgreSQL container is created once
/// and shared across all DataSync integration tests.
/// </summary>
[CollectionDefinition(Name)]
public class DataSyncTestContainerFixtureCollection : ICollectionFixture<DataSyncTestContainerFixture>
{
    public const string Name = nameof(DataSyncTestContainerFixtureCollection);
}
