using Dapper;
using Microsoft.Extensions.DependencyInjection;
using PseRestApi.Core.Services.DataSync;
using PseRestApi.Domain.Entities;
using Xunit;

namespace PseRestApi.IntegrationTests.DataSync;

/// <summary>
/// End-to-end integration tests for the DataSync pipeline using real services and PostgreSQL container.
/// Tests the full sync flow from data provider → strategy → staging/bulk copy.
/// </summary>
public class BaseSyncServiceTests : DataSyncIntegrationTestBase
{
    public BaseSyncServiceTests(DataSyncTestContainerFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public async Task Sync_StagedMode_FullPipeline_StoresDataInStaging()
    {
        // Arrange
        await EnsureStagingTableExistsAsync();

        var testData = new List<SecurityInfo>
        {
            new() { Symbol = "E2E1", CompanyId = 1, CompanyName = "E2E Test 1", SecurityName = "E2E Security 1", SecurityStatus = "Active" },
            new() { Symbol = "E2E2", CompanyId = 2, CompanyName = "E2E Test 2", SecurityName = "E2E Security 2", SecurityStatus = "Active" },
        };

        var options = new DataSyncOptions
        {
            SkipStaging = false,
            MergeCommand = "SELECT 1",
            CleanupCommand = "SELECT 1",
        };

        // Act - simulate the full staged sync pipeline
        var stagingStrategy = Factory.Services.GetRequiredService<StagedSyncStrategy>();
        await stagingStrategy.ExecuteAsync(testData.ToAsyncEnumerable(), options, NullLogger);

        // Assert
        var rows = await ReadStagingRowsAsync();
        rows.Count.Should().Be(2);
        rows.Should().OnlyContain(r => r.BatchId != Guid.Empty);
    }

    [Fact]
    public async Task Sync_DirectMode_FullPipeline_CopiesDataToTarget()
    {
        // Arrange
        await EnsureStagingTableExistsAsync();
        await using var conn = CreatePostgresConnection();
        await conn.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS "HistoricalTradingData" (
                "Id" SERIAL PRIMARY KEY,
                "SecurityId" INT,
                "Symbol" TEXT,
                "Currency" TEXT,
                "Change" DOUBLE PRECISION,
                "PercentChange" DOUBLE PRECISION,
                "Price" DOUBLE PRECISION,
                "TradeDate" DATE,
                "Value" DOUBLE PRECISION,
                "Volume" DOUBLE PRECISION,
                "Created" TIMESTAMPTZ,
                "LastModified" TIMESTAMPTZ
            )
            """);

        var directStrategy = Factory.Services.GetRequiredService<DirectBulkCopyStrategy>();
        var testData = new List<HistoricalTradingData>
        {
            new()
            {
                Symbol = "HIST1",
                Currency = "PHP",
                Price = 100.50,
                Volume = 1000,
                Value = 100500,
                TradeDate = DateOnly.FromDateTime(DateTime.Today),
            },
            new()
            {
                Symbol = "HIST2",
                Currency = "PHP",
                Price = 200.75,
                Volume = 2000,
                Value = 401500,
                TradeDate = DateOnly.FromDateTime(DateTime.Today),
            },
        };

        var options = new DataSyncOptions
        {
            SkipStaging = true,
            TargetTable = "HistoricalTradingData",
            ColumnMappings = new Dictionary<string, string>
            {
                { "SecurityId", "SecurityId" },
                { "Symbol", "Symbol" },
                { "Currency", "Currency" },
                { "Change", "Change" },
                { "PercentChange", "PercentChange" },
                { "Price", "Price" },
                { "TradeDate", "TradeDate" },
                { "Value", "Value" },
                { "Volume", "Volume" },
                { "Created", "Created" },
                { "LastModified", "LastModified" },
            },
        };

        // Act
        await directStrategy.ExecuteAsync(testData.ToAsyncEnumerable(), options, NullLogger);

        // Assert
        var count = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM \"HistoricalTradingData\"");
        count.Should().Be(2);
    }

    [Fact]
    public async Task Sync_StagedMode_PreservesBatchIdAcrossPipeline()
    {
        // Arrange
        await EnsureStagingTableExistsAsync();

        var testData = new List<SecurityInfo>
        {
            new() { Symbol = "BATCH", CompanyId = 1, CompanyName = "Batch Test", SecurityName = "Batch Security", SecurityStatus = "Active" },
        };

        var expectedBatchId = Guid.NewGuid();
        var options = new DataSyncOptions
        {
            SkipStaging = false,
            MergeCommand = "SELECT 1",
            CleanupCommand = "SELECT 1",
            BatchId = expectedBatchId,
        };

        // Act
        var stagingStrategy = Factory.Services.GetRequiredService<StagedSyncStrategy>();
        await stagingStrategy.ExecuteAsync(testData.ToAsyncEnumerable(), options, NullLogger);

        // Assert
        var rows = await ReadStagingRowsAsync();
        rows.Count.Should().Be(1);
        rows[0].BatchId.Should().Be(expectedBatchId);
    }

    [Fact]
    public async Task Sync_DirectMode_VerifiesDataIntegrity()
    {
        // Arrange
        await EnsureStagingTableExistsAsync();
        await using var conn = CreatePostgresConnection();
        await conn.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS "TargetSecurityInfo" (
                "Id" SERIAL PRIMARY KEY,
                "Symbol" TEXT,
                "CompanyId" INT,
                "CompanyName" TEXT,
                "SecurityName" TEXT,
                "SecurityStatus" TEXT,
                "Created" TIMESTAMPTZ,
                "LastModified" TIMESTAMPTZ
            )
            """);

        var directStrategy = Factory.Services.GetRequiredService<DirectBulkCopyStrategy>();
        var testData = new List<SecurityInfo>
        {
            new() { Symbol = "INT1", CompanyId = 42, CompanyName = "Integrity Corp", SecurityName = "Integrity Security", SecurityStatus = "Active" },
        };

        var options = new DataSyncOptions
        {
            SkipStaging = true,
            TargetTable = "TargetSecurityInfo",
            ColumnMappings = new Dictionary<string, string>
            {
                { "Symbol", "Symbol" },
                { "CompanyId", "CompanyId" },
                { "CompanyName", "CompanyName" },
                { "SecurityName", "SecurityName" },
                { "SecurityStatus", "SecurityStatus" },
                { "Created", "Created" },
                { "LastModified", "LastModified" },
            },
        };

        // Act
        await directStrategy.ExecuteAsync(testData.ToAsyncEnumerable(), options, NullLogger);

        // Assert
        var rows = await conn.QueryAsync<dynamic>("SELECT * FROM \"TargetSecurityInfo\"");
        rows.Count().Should().Be(1);
        var row = rows.First();
        ((string)row.Symbol).Should().Be("INT1");
        ((int)row.CompanyId).Should().Be(42);
        ((string)row.CompanyName).Should().Be("Integrity Corp");
    }

    [Fact]
    public async Task Sync_StagedMode_HandlesEmptyData()
    {
        // Arrange
        await EnsureStagingTableExistsAsync();
        var stagingStrategy = Factory.Services.GetRequiredService<StagedSyncStrategy>();

        var emptyData = Enumerable.Empty<SecurityInfo>();
        var options = new DataSyncOptions
        {
            SkipStaging = false,
            MergeCommand = "SELECT 1",
            CleanupCommand = "SELECT 1",
        };

        // Act
        var exception = await Record.ExceptionAsync(() =>
            stagingStrategy.ExecuteAsync(emptyData.ToAsyncEnumerable(), options, NullLogger));

        // Assert - should not throw, just produce empty staging
        exception.Should().BeNull();
        var rows = await ReadStagingRowsAsync();
        rows.Count.Should().Be(0);
    }

    [Fact]
    public async Task Sync_StagedMode_HandlesLargeDataset()
    {
        // Arrange
        await EnsureStagingTableExistsAsync();
        var stagingStrategy = Factory.Services.GetRequiredService<StagedSyncStrategy>();

        var testData = new List<SecurityInfo>();
        for (int i = 0; i < 200; i++)
        {
            testData.Add(new SecurityInfo
            {
                Symbol = $"LARGE{i:D4}",
                CompanyId = i + 1,
                CompanyName = $"Large Company {i}",
                SecurityName = $"Large Security {i}",
                SecurityStatus = "Active"
            });
        }

        var options = new DataSyncOptions
        {
            SkipStaging = false,
            MergeCommand = "SELECT 1",
            CleanupCommand = "SELECT 1",
        };

        // Act
        await stagingStrategy.ExecuteAsync(testData.ToAsyncEnumerable(), options, NullLogger);

        // Assert
        var rows = await ReadStagingRowsAsync();
        rows.Count.Should().Be(200);
    }

    [Fact]
    public async Task Sync_StagedMode_MultipleBatches_PreservesSeparateBatchIds()
    {
        // Arrange
        await EnsureStagingTableExistsAsync();
        var stagingStrategy = Factory.Services.GetRequiredService<StagedSyncStrategy>();

        var testData = new List<SecurityInfo>
        {
            new() { Symbol = "MULTI", CompanyId = 1, CompanyName = "Multi Test", SecurityName = "Multi Security", SecurityStatus = "Active" },
        };

        // Act
        var options1 = new DataSyncOptions { SkipStaging = false, MergeCommand = "SELECT 1", CleanupCommand = "SELECT 1" };
        await stagingStrategy.ExecuteAsync(testData.ToAsyncEnumerable(), options1, NullLogger);

        var options2 = new DataSyncOptions { SkipStaging = false, MergeCommand = "SELECT 1", CleanupCommand = "SELECT 1" };
        await stagingStrategy.ExecuteAsync(testData.ToAsyncEnumerable(), options2, NullLogger);

        // Assert
        var rows = await ReadStagingRowsAsync();
        rows.Count.Should().Be(2);
        rows.Select(r => r.BatchId).Distinct().Count().Should().Be(2);
    }
}
