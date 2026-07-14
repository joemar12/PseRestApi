using Dapper;
using Microsoft.Extensions.DependencyInjection;
using PseRestApi.Core.Services.DataSync;
using PseRestApi.Domain.Entities;
using Xunit;

namespace PseRestApi.IntegrationTests.DataSync;

/// <summary>
/// Integration tests for <see cref="DirectBulkCopyStrategy"/> against a real PostgreSQL container.
/// </summary>
public class DirectBulkCopyStrategyTests : DataSyncIntegrationTestBase
{
    public DirectBulkCopyStrategyTests(DataSyncTestContainerFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    private DirectBulkCopyStrategy GetDirectStrategy()
    {
        return Factory.Services.GetRequiredService<DirectBulkCopyStrategy>();
    }

    [Fact]
    public async Task ExecuteAsync_DirectMode_CopiesDataToTargetTable()
    {
        // Arrange
        await EnsureStagingTableExistsAsync();
        var strategy = GetDirectStrategy();

        // Create the target table
        await using var conn = CreatePostgresConnection();
        await conn.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS "SecurityInfo" (
                "SecurityId" SERIAL PRIMARY KEY,
                "Symbol" TEXT,
                "CompanyId" INT,
                "CompanyName" TEXT,
                "SecurityName" TEXT,
                "SecurityStatus" TEXT,
                "Created" TIMESTAMPTZ,
                "LastModified" TIMESTAMPTZ
            )
            """);

        var testData = new List<SecurityInfo>
        {
            new() { Symbol = "DIR1", CompanyId = 1, CompanyName = "Direct Test 1", SecurityName = "Direct Security 1", SecurityStatus = "Active" },
            new() { Symbol = "DIR2", CompanyId = 2, CompanyName = "Direct Test 2", SecurityName = "Direct Security 2", SecurityStatus = "Active" },
        };

        var options = new DataSyncOptions
        {
            SkipStaging = true,
            TargetTable = "SecurityInfo",
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
        await strategy.ExecuteAsync(testData.ToAsyncEnumerable(), options, TestNullLogger.Instance);

        // Assert
        var rows = await conn.QueryAsync<dynamic>("SELECT * FROM \"SecurityInfo\" ORDER BY \"SecurityId\"");
        rows.Count().Should().Be(2);
    }

    [Fact]
    public async Task ExecuteAsync_WithColumnMappings_AppliesMappingsCorrectly()
    {
        // Arrange
        await EnsureStagingTableExistsAsync();
        var strategy = GetDirectStrategy();

        // Create a target table with custom column names
        await using var conn = CreatePostgresConnection();
        await conn.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS "TargetEntity" (
                "Id" SERIAL PRIMARY KEY,
                "Sym" TEXT,
                "CompId" INT,
                "CompName" TEXT,
                "SecName" TEXT,
                "SecStatus" TEXT,
                "Created" TIMESTAMPTZ,
                "LastModified" TIMESTAMPTZ
            )
            """);

        var testData = new List<SecurityInfo>
        {
            new() { Symbol = "MAP1", CompanyId = 10, CompanyName = "Mapped Co", SecurityName = "Mapped Sec", SecurityStatus = "Active" },
        };

        var options = new DataSyncOptions
        {
            SkipStaging = true,
            TargetTable = "TargetEntity",
            ColumnMappings = new Dictionary<string, string>
            {
                { "Symbol", "Sym" },
                { "CompanyId", "CompId" },
                { "CompanyName", "CompName" },
                { "SecurityName", "SecName" },
                { "SecurityStatus", "SecStatus" },
            },
        };

        // Act
        await strategy.ExecuteAsync(testData.ToAsyncEnumerable(), options, TestNullLogger.Instance);

        // Assert
        var rows = await conn.QueryAsync<dynamic>("SELECT * FROM \"TargetEntity\"");
        rows.Count().Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_WithMultipleRecords_CopiesAllRecords()
    {
        // Arrange
        await EnsureStagingTableExistsAsync();
        var strategy = GetDirectStrategy();

        await using var conn = CreatePostgresConnection();
        await conn.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS "BulkTarget" (
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

        var testData = new List<SecurityInfo>();
        for (int i = 0; i < 50; i++)
        {
            testData.Add(new SecurityInfo
            {
                Symbol = $"BULK{i:D3}",
                CompanyId = i + 1,
                CompanyName = $"Bulk Company {i}",
                SecurityName = $"Bulk Security {i}",
                SecurityStatus = "Active"
            });
        }

        var options = new DataSyncOptions
        {
            SkipStaging = true,
            TargetTable = "BulkTarget",
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
        await strategy.ExecuteAsync(testData.ToAsyncEnumerable(), options, TestNullLogger.Instance);

        // Assert
        var count = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM \"BulkTarget\"");
        count.Should().Be(50);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullTargetTable_ThrowsException()
    {
        // Arrange
        var strategy = GetDirectStrategy();
        var testData = new List<SecurityInfo>
        {
            new() { Symbol = "ERR", CompanyId = 1, CompanyName = "Error Test", SecurityName = "Error Security", SecurityStatus = "Active" },
        };

        var options = new DataSyncOptions
        {
            SkipStaging = true,
            TargetTable = null,
        };

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => strategy.ExecuteAsync(
            testData.ToAsyncEnumerable(), options, TestNullLogger.Instance));

        exception.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_EmptyData_DoesNotThrow()
    {
        // Arrange
        await EnsureStagingTableExistsAsync();
        var strategy = GetDirectStrategy();

        await using var conn = CreatePostgresConnection();
        await conn.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS "EmptyTarget" (
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

        var testData = Enumerable.Empty<SecurityInfo>();

        var options = new DataSyncOptions
        {
            SkipStaging = true,
            TargetTable = "EmptyTarget",
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

        // Act - should not throw even with empty data
        var exception = await Record.ExceptionAsync(() => strategy.ExecuteAsync(
            testData.ToAsyncEnumerable(), options, TestNullLogger.Instance));

        exception.Should().BeNull();
    }
}
