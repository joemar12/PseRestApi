using Microsoft.Extensions.DependencyInjection;
using PseRestApi.Core.Services.DataSync;
using PseRestApi.Domain.Entities;
using Xunit;

namespace PseRestApi.IntegrationTests.DataSync;

/// <summary>
/// Integration tests for <see cref="StagedSyncStrategy"/> against a real PostgreSQL container.
/// </summary>
public class StagedSyncStrategyTests : DataSyncIntegrationTestBase
{
    public StagedSyncStrategyTests(DataSyncTestContainerFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    private StagedSyncStrategy GetStagedStrategy()
    {
        return Factory.Services.GetRequiredService<StagedSyncStrategy>();
    }

    [Fact]
    public async Task ExecuteAsync_StagedMode_StoresDataInStagingAndExecutesMerge()
    {
        // Arrange
        await EnsureStagingTableExistsAsync();
        var strategy = GetStagedStrategy();
        var testData = new List<SecurityInfo>
        {
            new() { Symbol = "MERGE", CompanyId = 1, CompanyName = "Merge Test", SecurityName = "Merge Security", SecurityStatus = "Active" },
        };

        var options = new DataSyncOptions
        {
            SkipStaging = false,
            MergeCommand = "SELECT 1",
            CleanupCommand = "SELECT 1",
            BatchId = Guid.NewGuid(),
        };

        // Act
        await strategy.ExecuteAsync(testData.ToAsyncEnumerable(), options, TestNullLogger.Instance);

        // Assert - data should be in staging table
        var rows = await ReadStagingRowsAsync();
        rows.Count.Should().Be(1);
        rows[0].BatchId.Should().Be(options.BatchId);
    }

    [Fact]
    public async Task ExecuteAsync_MultipleDataItems_StoresAllInStaging()
    {
        // Arrange
        await EnsureStagingTableExistsAsync();
        var strategy = GetStagedStrategy();
        var testData = new List<SecurityInfo>
        {
            new() { Symbol = "A", CompanyId = 1, CompanyName = "Company A", SecurityName = "Security A", SecurityStatus = "Active" },
            new() { Symbol = "B", CompanyId = 2, CompanyName = "Company B", SecurityName = "Security B", SecurityStatus = "Active" },
            new() { Symbol = "C", CompanyId = 3, CompanyName = "Company C", SecurityName = "Security C", SecurityStatus = "Active" },
        };

        var options = new DataSyncOptions
        {
            SkipStaging = false,
            MergeCommand = "SELECT 1",
            CleanupCommand = "SELECT 1",
            BatchId = Guid.NewGuid(),
        };

        // Act
        await strategy.ExecuteAsync(testData.ToAsyncEnumerable(), options, TestNullLogger.Instance);

        // Assert
        var rows = await ReadStagingRowsAsync();
        rows.Count.Should().Be(3);
        rows.Should().OnlyContain(r => r.BatchId == options.BatchId);
    }

    [Fact]
    public async Task ExecuteAsync_WithMergeAndCleanupCommands_ExecutesBoth()
    {
        // Arrange
        await EnsureStagingTableExistsAsync();
        var strategy = GetStagedStrategy();
        var testData = new List<SecurityInfo>
        {
            new() { Symbol = "BOTH", CompanyId = 1, CompanyName = "Both Test", SecurityName = "Both Security", SecurityStatus = "Active" },
        };

        var batchId = Guid.NewGuid();
        var options = new DataSyncOptions
        {
            SkipStaging = false,
            MergeCommand = "SELECT 1 AS result",
            CleanupCommand = "SELECT 1 AS result",
            BatchId = batchId,
        };

        // Act
        await strategy.ExecuteAsync(testData.ToAsyncEnumerable(), options, TestNullLogger.Instance);

        // Assert - no exception means both commands executed successfully
        var rows = await ReadStagingRowsAsync();
        rows.Count.Should().Be(1);
        rows[0].BatchId.Should().Be(batchId);
    }

    [Fact]
    public async Task ExecuteAsync_MergeCommandFails_StillExecutesCleanup()
    {
        // Arrange
        await EnsureStagingTableExistsAsync();
        var strategy = GetStagedStrategy();
        var testData = new List<SecurityInfo>
        {
            new() { Symbol = "FAIL", CompanyId = 1, CompanyName = "Fail Test", SecurityName = "Fail Security", SecurityStatus = "Active" },
        };

        var batchId = Guid.NewGuid();

        // Create a merge command that will fail and a cleanup that tracks execution
        var options = new DataSyncOptions
        {
            SkipStaging = false,
            MergeCommand = "SELECT * FROM NonExistentTable", // This will fail
            CleanupCommand = "SELECT 1",
            BatchId = batchId,
        };

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => strategy.ExecuteAsync(
            testData.ToAsyncEnumerable(), options, TestNullLogger.Instance));

        exception.Should().BeOfType<SyncStrategyException>();
        exception!.Message.Should().Contain("Merge command failed");

        // The staging should still have the data
        var rows = await ReadStagingRowsAsync();
        rows.Count.Should().Be(1);
        rows[0].BatchId.Should().Be(batchId);
    }

    [Fact]
    public async Task ExecuteAsync_NoMergeCommand_OnlyStagesData()
    {
        // Arrange
        await EnsureStagingTableExistsAsync();
        var strategy = GetStagedStrategy();
        var testData = new List<SecurityInfo>
        {
            new() { Symbol = "NOMERGE", CompanyId = 1, CompanyName = "No Merge", SecurityName = "No Merge Security", SecurityStatus = "Active" },
        };

        var batchId = Guid.NewGuid();
        var options = new DataSyncOptions
        {
            SkipStaging = false,
            MergeCommand = null, // No merge command
            CleanupCommand = null, // No cleanup command
            BatchId = batchId,
        };

        // Act
        await strategy.ExecuteAsync(testData.ToAsyncEnumerable(), options, TestNullLogger.Instance);

        // Assert - data should be staged but no merge/cleanup executed
        var rows = await ReadStagingRowsAsync();
        rows.Count.Should().Be(1);
        rows[0].BatchId.Should().Be(batchId);
    }

    [Fact]
    public async Task ExecuteAsync_StagesDataWithCorrectBatchId()
    {
        // Arrange
        await EnsureStagingTableExistsAsync();
        var strategy = GetStagedStrategy();
        var testData = new List<SecurityInfo>
        {
            new() { Symbol = "BATCHID", CompanyId = 1, CompanyName = "Batch Id Test", SecurityName = "Batch Id Security", SecurityStatus = "Active" },
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
        await strategy.ExecuteAsync(testData.ToAsyncEnumerable(), options, TestNullLogger.Instance);

        // Assert
        var rows = await ReadStagingRowsAsync();
        rows.Count.Should().Be(1);
        rows[0].BatchId.Should().Be(expectedBatchId);
    }
}
