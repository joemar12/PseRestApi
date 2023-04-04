namespace PseRestApi.Core.Services.DataSync;

public class DataSyncOptionsBuilder
{
    private readonly DataSyncOptions _options;
    public DataSyncOptionsBuilder()
    {
        _options = new DataSyncOptions();
    }

    public StagedDataSyncOptionsBuilder WithStaging()
    {
        _options.SkipStaging = false;
        return new StagedDataSyncOptionsBuilder(this);
    }

    public DirectDataSyncOptionsBuilder SkipStaging()
    {
        _options.SkipStaging = true;
        return new DirectDataSyncOptionsBuilder(this);
    }

    public DataSyncOptions Build()
    {
        return _options;
    }
}

public class StagedDataSyncOptionsBuilder
{
    private readonly DataSyncOptions _options;

    public StagedDataSyncOptionsBuilder(DataSyncOptionsBuilder builder)
    {
        _options = builder.Build();
    }

    public StagedDataSyncOptionsBuilder WithMergeCommand(string mergeCommand)
    {
        _options.MergeCommand = mergeCommand;
        return this;
    }

    public StagedDataSyncOptionsBuilder WithCleanupCommand(string cleanupCommand)
    {
        _options.CleanupCommand = cleanupCommand;
        return this;
    }

    public StagedDataSyncOptionsBuilder WithBatchId(Guid batchId)
    {
        _options.BatchId = batchId;
        return this;
    }

    public DataSyncOptions Build()
    {
        return _options;
    }
}

public class DirectDataSyncOptionsBuilder
{
    private readonly DataSyncOptions _options;

    public DirectDataSyncOptionsBuilder(DataSyncOptionsBuilder builder)
    {
        _options = builder.Build();
    }

    public DirectDataSyncOptionsBuilder WithTargetTable(string targetTable)
    {
        _options.TargetTable = targetTable;
        return this;
    }

    public DirectDataSyncOptionsBuilder WithColumnMappings(Dictionary<string, string> columnMappings)
    {
        _options.ColumnMappings = columnMappings;
        return this;
    }

    public DataSyncOptions Build()
    {
        return _options;
    }
}
