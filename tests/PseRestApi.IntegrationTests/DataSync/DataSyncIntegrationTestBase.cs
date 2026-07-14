using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using PseRestApi.Core.Common;
using PseRestApi.Core.Services.DataSync;
using Xunit;

namespace PseRestApi.IntegrationTests.DataSync;

/// <summary>
/// Base class for DataSync integration tests. Provides a <see cref="WebApplicationFactory{T}"/>
/// configured with a real PostgreSQL Testcontainer and real DataSync service implementations.
/// </summary>
[Collection(DataSyncTestContainerFixtureCollection.Name)]
public abstract class DataSyncIntegrationTestBase : IAsyncLifetime
{
    protected readonly DataSyncTestContainerFixture Fixture;
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly ITestOutputHelper Output;

    /// <summary>
    /// Provides a null logger instance for tests that don't need logging output.
    /// </summary>
    protected ILogger<object> NullLogger => NullLogger<object>.Instance;

    /// <summary>
    /// Creates a test-specific connection provider that points to the PostgreSQL container.
    /// </summary>
    private class TestDbConnectionProvider : IDbConnectionProvider
    {
        private readonly string _connectionString;
        private readonly ILogger<TestDbConnectionProvider>? _logger;

        public TestDbConnectionProvider(string connectionString, ILogger<TestDbConnectionProvider>? logger = null)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public System.Data.IDbConnection CreateConnection()
        {
            _logger?.LogDebug("Creating PostgreSQL connection to test container");
            return new NpgsqlConnection(_connectionString);
        }
    }

    protected DataSyncIntegrationTestBase(DataSyncTestContainerFixture fixture, ITestOutputHelper output)
    {
        Fixture = fixture;
        Output = output;

        Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Debug);
            }).ConfigureServices(services =>
            {
                // Remove the production DbConnectionProvider
                var connectionProviderDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IDbConnectionProvider));
                if (connectionProviderDescriptor != null)
                {
                    services.Remove(connectionProviderDescriptor);
                }

                // Register test-specific connection provider pointing to the container
                services.AddSingleton<IDbConnectionProvider>(
                    new TestDbConnectionProvider(Fixture.ConnectionString));

                // Remove the production NpgsqlBulkCopyService and re-register with test logger
                var bulkCopyDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IBulkCopyService));
                if (bulkCopyDescriptor != null)
                {
                    services.Remove(bulkCopyDescriptor);
                }
                services.AddTransient<IBulkCopyService, NpgsqlBulkCopyService>();

                // Remove the staged/direct strategies and re-register
                var stagedDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(StagedSyncStrategy));
                if (stagedDescriptor != null)
                {
                    services.Remove(stagedDescriptor);
                }
                services.AddTransient<StagedSyncStrategy>();

                var directDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DirectBulkCopyStrategy));
                if (directDescriptor != null)
                {
                    services.Remove(directDescriptor);
                }
                services.AddTransient<DirectBulkCopyStrategy>();

                // Remove the factory and re-register
                var factoryDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(ISyncStrategyFactory));
                if (factoryDescriptor != null)
                {
                    services.Remove(factoryDescriptor);
                }
                services.AddTransient<ISyncStrategyFactory, SyncStrategyFactory>();

                // Remove the staging service and re-register
                var stagingDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(ISyncDataStagingService));
                if (stagingDescriptor != null)
                {
                    services.Remove(stagingDescriptor);
                }
                services.AddTransient<ISyncDataStagingService, SyncDataStagingService>();

                // Also register the concrete NpgsqlBulkCopyService for direct access in tests
                services.AddTransient<NpgsqlBulkCopyService>();
            });
        });
    }

    /// <summary>
    /// Creates an <see cref="HttpClient"/> configured to talk to the test server.
    /// </summary>
    protected HttpClient CreateClient()
    {
        return Factory.CreateClient();
    }

    /// <summary>
    /// Creates a fresh <see cref="NpgsqlConnection"/> to the test PostgreSQL container.
    /// </summary>
    protected NpgsqlConnection CreatePostgresConnection()
    {
        var connection = new NpgsqlConnection(Fixture.ConnectionString);
        connection.Open();
        return connection;
    }

    /// <summary>
    /// Truncates the SyncBatchData table to clean up between tests.
    /// </summary>
    protected async Task TruncateStagingTableAsync()
    {
        await using var connection = CreatePostgresConnection();
        await connection.ExecuteAsync("TRUNCATE TABLE \"SyncBatchData\" CASCADE");
    }

    /// <summary>
    /// Drops the SyncBatchData table if it exists.
    /// </summary>
    protected async Task DropStagingTableAsync()
    {
        await using var connection = CreatePostgresConnection();
        await connection.ExecuteAsync("DROP TABLE IF EXISTS \"SyncBatchData\" CASCADE");
    }

    /// <summary>
    /// Creates the SyncBatchData table with the correct schema.
    /// </summary>
    protected async Task EnsureStagingTableExistsAsync()
    {
        await using var connection = CreatePostgresConnection();
        await connection.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS "SyncBatchData" (
                "Id" SERIAL PRIMARY KEY,
                "BatchId" UUID NOT NULL,
                "Data" TEXT NOT NULL,
                "Created" TIMESTAMPTZ NOT NULL,
                "LastModified" TIMESTAMPTZ
            )
            """);
    }

    /// <summary>
    /// Counts the rows in the SyncBatchData table.
    /// </summary>
    protected async Task<int> CountStagingRowsAsync()
    {
        await using var connection = CreatePostgresConnection();
        return await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM \"SyncBatchData\"");
    }

    /// <summary>
    /// Reads all rows from the SyncBatchData table.
    /// </summary>
    protected async Task<List<(Guid BatchId, string Data, DateTime Created)>> ReadStagingRowsAsync()
    {
        await using var connection = CreatePostgresConnection();
        var rows = new List<(Guid BatchId, string Data, DateTime Created)>();
        var results = await connection.QueryAsync<(Guid BatchId, string Data, DateTime Created)>(
            "SELECT \"BatchId\", \"Data\", \"Created\" FROM \"SyncBatchData\" ORDER BY \"Id\"");
        foreach (var row in results)
        {
            rows.Add(row);
        }
        return rows;
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        await TruncateStagingTableAsync();
        await Factory.DisposeAsync();
    }
}
