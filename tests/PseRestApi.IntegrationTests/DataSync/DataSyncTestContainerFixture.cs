using Testcontainers.PostgreSql;
using Xunit;

namespace PseRestApi.IntegrationTests.DataSync;

/// <summary>
/// Shared test container fixture for PostgreSQL-based DataSync integration tests.
/// A single container is created once and shared across all tests in the collection.
/// </summary>
public class DataSyncTestContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres;

    public DataSyncTestContainerFixture()
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:17-alpine")
            .WithDatabase("pse_rest_api_test")
            .WithUsername("postgres")
            .WithPassword("TestPass123!")
            .Build();
    }

    public string ConnectionString => _postgres.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }
}
