global using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using PseRestApi.Core.Services.DataSync;
using PseRestApi.Domain.Entities;
using Xunit;

namespace PseRestApi.IntegrationTests.DataSync;

/// <summary>
/// Integration tests for <see cref="SyncDataStagingService"/> against a real PostgreSQL container.
/// </summary>
public class SyncDataStagingServiceTests : DataSyncIntegrationTestBase
{
    public SyncDataStagingServiceTests(DataSyncTestContainerFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    private ISyncDataStagingService GetStagingService()
    {
        return Factory.Services.GetRequiredService<ISyncDataStagingService>();
    }

    [Fact]
    public async Task Stage_WithValidData_InsertsRowsIntoStagingTable()
    {
        // Arrange
        await EnsureStagingTableExistsAsync();
        var stagingService = GetStagingService();
        var testData = new List<SecurityInfo>
        {
            new() { Symbol = "ABC", CompanyId = 1, CompanyName = "Company A", SecurityName = "Security A", SecurityStatus = "Active" },
            new() { Symbol = "DEF", CompanyId = 2, CompanyName = "Company B", SecurityName = "Security B", SecurityStatus = "Active" },
            new() { Symbol = "GHI", CompanyId = 3, CompanyName = "Company C", SecurityName = "Security C", SecurityStatus = "Inactive" },
        };

        // Act
        var batchId = await stagingService.Stage(testData.ToAsyncEnumerable());

        // Assert
        batchId.Should().NotBeEmpty();
        var rows = await ReadStagingRowsAsync();
        rows.Count.Should().Be(3);
        rows.Should().OnlyContain(r => r.BatchId == batchId);
        rows.Should().OnlyContain(r => r.Created != default);
    }

    [Fact]
    public async Task Stage_WithEmptyData_ReturnsNewBatchId()
    {
        // Arrange
        await EnsureStagingTableExistsAsync();
        var stagingService = GetStagingService();
        var emptyData = Enumerable.Empty<SecurityInfo>().ToAsyncEnumerable();

        // Act
        var batchId = await stagingService.Stage(emptyData);

        // Assert
        batchId.Should().NotBeEmpty();
        var rows = await ReadStagingRowsAsync();
        rows.Count.Should().Be(0);
    }

    [Fact]
    public async Task Stage_WithCustomBatchId_UsesProvidedBatchId()
    {
        // Arrange
        await EnsureStagingTableExistsAsync();
        var stagingService = GetStagingService();
        var customBatchId = Guid.NewGuid();
        var testData = new List<SecurityInfo>
        {
            new() { Symbol = "XYZ", CompanyId = 1, CompanyName = "Test Co", SecurityName = "Test Security", SecurityStatus = "Active" },
        };

        // Act
        var resultBatchId = await stagingService.Stage(testData.ToAsyncEnumerable(), customBatchId);

        // Assert
        resultBatchId.Should().Be(customBatchId);
        var rows = await ReadStagingRowsAsync();
        rows.Count.Should().Be(1);
        rows[0].BatchId.Should().Be(customBatchId);
    }

    [Fact]
    public async Task Stage_WithNullBatchId_GeneratesNewBatchId()
    {
        // Arrange
        await EnsureStagingTableExistsAsync();
        var stagingService = GetStagingService();
        var testData = new List<SecurityInfo>
        {
            new() { Symbol = "AAA", CompanyId = 1, CompanyName = "Test A", SecurityName = "Test", SecurityStatus = "Active" },
        };

        // Act
        var batchId = await stagingService.Stage(testData.ToAsyncEnumerable(), null);

        // Assert
        batchId.Should().NotBeEmpty();
        var rows = await ReadStagingRowsAsync();
        rows.Count.Should().Be(1);
        rows[0].BatchId.Should().Be(batchId);
    }

    [Fact]
    public async Task Stage_WithEmptyBatchId_GeneratesNewBatchId()
    {
        // Arrange
        await EnsureStagingTableExistsAsync();
        var stagingService = GetStagingService();
        var testData = new List<SecurityInfo>
        {
            new() { Symbol = "BBB", CompanyId = 2, CompanyName = "Test B", SecurityName = "Test", SecurityStatus = "Active" },
        };

        // Act
        var batchId = await stagingService.Stage(testData.ToAsyncEnumerable(), Guid.Empty);

        // Assert
        batchId.Should().NotBeEmpty();
        var rows = await ReadStagingRowsAsync();
        rows.Count.Should().Be(1);
        rows[0].BatchId.Should().Be(batchId);
    }

    [Fact]
    public async Task Stage_WithMultipleCalls_GeneratesDifferentBatchIds()
    {
        // Arrange
        await EnsureStagingTableExistsAsync();
        var stagingService = GetStagingService();
        var testData = new List<SecurityInfo>
        {
            new() { Symbol = "CCC", CompanyId = 3, CompanyName = "Test C", SecurityName = "Test", SecurityStatus = "Active" },
        };

        // Act
        var batchId1 = await stagingService.Stage(testData.ToAsyncEnumerable());
        var batchId2 = await stagingService.Stage(testData.ToAsyncEnumerable());

        // Assert
        batchId1.Should().NotBe(batchId2);
        var rows = await ReadStagingRowsAsync();
        rows.Count.Should().Be(2);
        rows.Select(r => r.BatchId).Should().ContainInOrder(batchId1, batchId2);
    }

    [Fact]
    public async Task Stage_DataIsProperlySerialized()
    {
        // Arrange
        await EnsureStagingTableExistsAsync();
        var stagingService = GetStagingService();
        var testData = new List<SecurityInfo>
        {
            new() { Symbol = "SYN", CompanyId = 10, CompanyName = "Symbol Corp", SecurityName = "Symbol Security", SecurityStatus = "Active" },
        };

        // Act
        await stagingService.Stage(testData.ToAsyncEnumerable());

        // Assert
        var rows = await ReadStagingRowsAsync();
        rows.Count.Should().Be(1);
        rows[0].Data.Should().Contain("\"Symbol\":\"SYN\"");
        rows[0].Data.Should().Contain("\"CompanyId\":10");
        rows[0].Data.Should().Contain("\"CompanyName\":\"Symbol Corp\"");
    }

    [Fact]
    public async Task Stage_WithLargeDataSet_InsertsAllRows()
    {
        // Arrange
        await EnsureStagingTableExistsAsync();
        var stagingService = GetStagingService();
        var testData = new List<SecurityInfo>();
        for (int i = 0; i < 100; i++)
        {
            testData.Add(new SecurityInfo
            {
                Symbol = $"SYM{i:D4}",
                CompanyId = i + 1,
                CompanyName = $"Company {i}",
                SecurityName = $"Security {i}",
                SecurityStatus = "Active"
            });
        }

        // Act
        var batchId = await stagingService.Stage(testData.ToAsyncEnumerable());

        // Assert
        var rows = await ReadStagingRowsAsync();
        rows.Count.Should().Be(100);
        rows.Should().OnlyContain(r => r.BatchId == batchId);
    }
}
