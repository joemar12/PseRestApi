namespace PseRestApi.Core.Services.DataSync;

public class DataSyncOptionsBuilder
{
    private readonly DataSyncOptions _options = new();

    /// <summary>
    /// Configures staged sync (default). Pass true to skip staging and use direct bulk copy instead.
    /// </summary>
    public DataSyncOptionsBuilder WithSkipStaging(bool skipStaging = false)
    {
        _options.SkipStaging = skipStaging;
        return this;
    }

    public DataSyncOptionsBuilder WithBatchId(Guid? batchId = null)
    {
        _options.BatchId = batchId ?? Guid.NewGuid();
        return this;
    }

    public DataSyncOptionsBuilder WithMergeCommand(string? mergeCommand)
    {
        _options.MergeCommand = mergeCommand;
        return this;
    }

    public DataSyncOptionsBuilder WithCleanupCommand(string? cleanupCommand)
    {
        _options.CleanupCommand = cleanupCommand;
        return this;
    }

    public DataSyncOptionsBuilder WithTargetTable(string? targetTable)
    {
        _options.TargetTable = targetTable;
        return this;
    }

    public DataSyncOptionsBuilder WithColumnMappings(Dictionary<string, string> columnMappings)
    {
        _options.ColumnMappings = columnMappings;
        return this;
    }

    public DataSyncOptions Build() => _options;
}
