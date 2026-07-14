using Dapper;
using Microsoft.Extensions.DependencyInjection;
using PseRestApi.Core.Services.DataSync;
using PseRestApi.Domain.Entities;
using Xunit;

namespace PseRestApi.IntegrationTests.DataSync;

/// <summary>
/// Integration tests for <see cref="NpgsqlBulkCopyService"/> against a real PostgreSQL container.
/// </summary>
public class NpgsqlBulkCopyServiceTests : DataSyncIntegrationTestBase
{
    private readonly Dictionary<string, string> _defaultColumnMappings = new()
    {
        { "BatchId", "BatchId" },
        { "Data", "Data" },
        { "Created", "Created" },
    };

    public NpgsqlBulkCopyServiceTests(DataSyncTestContainerFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    private NpgsqlBulkCopyService GetBulkCopyService()
    {
        return Factory.Services.GetRequiredService<NpgsqlBulkCopyService>();
    }

    [Fact]
    public async Task CopyAsync_WithValidData_InsertsAllRows()
    {
        // Arrange
        await EnsureStagingTableExistsAsync();
        var bulkCopyService = GetBulkCopyService();
        var testData = new List<SyncBatchData>
        {
            new() { BatchId = Guid.NewGuid(), Data = "{\"Symbol\":\"A\"}", Created = DateTime.UtcNow },
            new() { BatchId = Guid.NewGuid(), Data = "{\"Symbol\":\"B\"}", Created = DateTime.UtcNow },
            new() { BatchId = Guid.NewGuid(), Data = "{\"Symbol\":\"C\"}", Created = DateTime.UtcNow },
        };

        // Act
        await bulkCopyService.CopyAsync(testData.ToAsyncEnumerable(), "SyncBatchData", _defaultColumnMappings);

        // Assert
        var count = await CountStagingRowsAsync();
        count.Should().Be(3);
    }

    [Fact]
    public async Task CopyAsync_WithColumnMappings_RenamesColumnsCorrectly()
    {
        // Arrange
        // Create a table with renamed columns
        await using var conn = CreatePostgresConnection();
        await conn.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS "CustomStaging" (
                "Id" SERIAL PRIMARY KEY,
                "CustomBatchId" UUID NOT NULL,
                "CustomData" TEXT NOT NULL,
                "CustomCreated" TIMESTAMPTZ NOT NULL
            )
            """);

        var bulkCopyService = GetBulkCopyService();
        var testData = new List<SyncBatchData>
        {
            new() { BatchId = Guid.NewGuid(), Data = "TestData1", Created = DateTime.UtcNow },
            new() { BatchId = Guid.NewGuid(), Data = "TestData2", Created = DateTime.UtcNow },
        };

        var customColumnMappings = new Dictionary<string, string>
        {
            { "BatchId", "CustomBatchId" },
            { "Data", "CustomData" },
            { "Created", "CustomCreated" },
        };

        // Act
        await bulkCopyService.CopyAsync(testData.ToAsyncEnumerable(), "CustomStaging", customColumnMappings);

        // Assert
        var count = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM \"CustomStaging\"");
        count.Should().Be(2);
    }

    [Fact]
    public async Task CopyAsync_WithLargeDataset_InsertsAllRows()
    {
        // Arrange
        await EnsureStagingTableExistsAsync();
        var bulkCopyService = GetBulkCopyService();
        var testData = new List<SyncBatchData>();
        for (int i = 0; i < 500; i++)
        {
            testData.Add(new SyncBatchData
            {
                BatchId = Guid.NewGuid(),
                Data = $"{{\"Index\":{i},\"Symbol\":\"SYM{i:D4}\"}}",
                Created = DateTime.UtcNow,
            });
        }

        // Act
        await bulkCopyService.CopyAsync(testData.ToAsyncEnumerable(), "SyncBatchData", _defaultColumnMappings);

        // Assert
        var count = await CountStagingRowsAsync();
        count.Should().Be(500);
    }

    [Fact]
    public async Task CopyAsync_WithNoColumnMappings_UsesPropertyNames()
    {
        // Arrange
        await EnsureStagingTableExistsAsync();
        var bulkCopyService = GetBulkCopyService();
        var testData = new List<SyncBatchData>
        {
            new() { BatchId = Guid.NewGuid(), Data = "NoMappings", Created = DateTime.UtcNow },
        };

        // Act
        await bulkCopyService.CopyAsync(testData.ToAsyncEnumerable(), "SyncBatchData");

        // Assert
        var count = await CountStagingRowsAsync();
        count.Should().Be(1);
    }

    [Fact]
    public async Task CopyAsync_WithPartialColumnMappings_OnlyMapsSpecifiedColumns()
    {
        // Arrange
        await EnsureStagingTableExistsAsync();
        var bulkCopyService = GetBulkCopyService();

        // Create a custom table where unmapped columns are nullable
        await using var conn = CreatePostgresConnection();
        await conn.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS "PartialMapTarget" (
                "BatchId" UUID PRIMARY KEY,
                "Data" TEXT,
                "Created" TIMESTAMPTZ
            )
            """);

        var testData = new List<SyncBatchData>
        {
            new() { BatchId = Guid.NewGuid(), Data = "PartialMap", Created = DateTime.UtcNow },
        };

        // Only map BatchId, skip Data and Created
        var columnMappings = new Dictionary<string, string>
        {
            { "BatchId", "BatchId" },
        };

        // Act
        await bulkCopyService.CopyAsync(testData.ToAsyncEnumerable(), "PartialMapTarget", columnMappings);

        // Assert
        var count = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM \"PartialMapTarget\"");
        count.Should().Be(1);

        // Verify the BatchId was written
        var rows = await conn.QueryAsync<dynamic>("SELECT * FROM \"PartialMapTarget\"");
        rows.Count().Should().Be(1);
        ((Guid)(object)rows.First().BatchId).Should().NotBeEmpty();
    }

    [Fact]
    public async Task CopyAsync_WithEmptyData_DoesNotThrow()
    {
        // Arrange
        await EnsureStagingTableExistsAsync();
        var bulkCopyService = GetBulkCopyService();
        var testData = new List<SyncBatchData>();

        // Act - should not throw even with empty data
        var exception = await Record.ExceptionAsync(() =>
            bulkCopyService.CopyAsync(testData.ToAsyncEnumerable(), "SyncBatchData", _defaultColumnMappings));

        exception.Should().BeNull();
    }

    [Fact]
    public async Task CopyAsync_DataIntegrity_VerifiesWrittenData()
    {
        // Arrange
        await EnsureStagingTableExistsAsync();
        var bulkCopyService = GetBulkCopyService();
        var expectedBatchId = Guid.NewGuid();
        var expectedData = "{\"TestKey\":\"TestValue\",\"Number\":42}";
        var testData = new List<SyncBatchData>
        {
            new() { BatchId = expectedBatchId, Data = expectedData, Created = DateTime.UtcNow },
        };

        // Act
        await bulkCopyService.CopyAsync(testData.ToAsyncEnumerable(), "SyncBatchData", _defaultColumnMappings);

        // Assert
        var rows = await ReadStagingRowsAsync();
        rows.Count.Should().Be(1);
        rows[0].BatchId.Should().Be(expectedBatchId);
        rows[0].Data.Should().Be(expectedData);
    }
}
