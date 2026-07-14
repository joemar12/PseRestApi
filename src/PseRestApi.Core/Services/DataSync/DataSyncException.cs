namespace PseRestApi.Core.Services.DataSync;

/// <summary>
/// Base exception for all DataSync errors.
/// </summary>
public abstract class DataSyncException : Exception
{
    protected DataSyncException(string message) : base(message) { }

    protected DataSyncException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Thrown when data fetching from the source fails.
/// </summary>
public class DataProviderException : DataSyncException
{
    public DataProviderException(string message) : base(message) { }

    public DataProviderException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Thrown when staging data to the intermediate table fails.
/// </summary>
public class StagingException : DataSyncException
{
    public StagingException(string message) : base(message) { }

    public StagingException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Thrown when strategy execution (merge/cleanup) fails.
/// </summary>
public class SyncStrategyException : DataSyncException
{
    public SyncStrategyException(string message) : base(message) { }

    public SyncStrategyException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Thrown when bulk copy operations fail.
/// </summary>
public class BulkCopyException : DataSyncException
{
    public BulkCopyException(string message) : base(message) { }

    public BulkCopyException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Thrown when orchestration or configuration fails.
/// </summary>
public class OrchestrationException : DataSyncException
{
    public OrchestrationException(string message) : base(message) { }

    public OrchestrationException(string message, Exception innerException)
        : base(message, innerException) { }
}
