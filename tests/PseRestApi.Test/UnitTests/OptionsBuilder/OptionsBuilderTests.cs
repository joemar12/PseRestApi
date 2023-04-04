using FluentAssertions;
using FluentAssertions.Execution;
using PseRestApi.Core.Services.DataSync;
using Xunit;

namespace PseRestApi.Test.UnitTests.OptionsBuilder;
public class OptionsBuilderTests
{
    [Fact]
    public void OptionsBuilderShouldBuildOptionsForSyncWithStaging()
    {
        var builder = new DataSyncOptionsBuilder();
        var command = "test";
        var cleanupCommand = "cleanup";
        var batchID = Guid.NewGuid();
        var options = builder
            .WithStaging()
            .WithMergeCommand(command)
            .WithCleanupCommand(cleanupCommand)
            .WithBatchId(batchID)
            .Build();

        using (new AssertionScope())
        {
            options.SkipStaging.Should().Be(false);
            options.MergeCommand.Should().Be(command);
            options.CleanupCommand.Should().Be(cleanupCommand);
            options.BatchId.Should().Be(batchID);
            options.ColumnMappings.Should().BeNullOrEmpty();
            options.TargetTable.Should().BeNullOrEmpty();
        }
    }

    [Fact]
    public void OptionsBuilderShouldBuildOptionsForDirectSync()
    {
        var builder = new DataSyncOptionsBuilder();
        var targetTable = "test";
        var columnMappings = new Dictionary<string, string>() { { "test", "test" } };
        var options = builder
            .SkipStaging()
            .WithTargetTable(targetTable)
            .WithColumnMappings(columnMappings)
            .Build();

        using (new AssertionScope())
        {
            options.SkipStaging.Should().Be(true);
            options.TargetTable.Should().Be(targetTable);
            options.ColumnMappings.Should().BeEquivalentTo(columnMappings);
            options.MergeCommand.Should().BeNullOrEmpty();
            options.CleanupCommand.Should().BeNullOrEmpty();
            options.BatchId.Should().BeEmpty();
        }
    }
}
