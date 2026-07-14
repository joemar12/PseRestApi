using Microsoft.Extensions.Logging;

namespace PseRestApi.Core.Services.DataSync;

public class DirectBulkCopyStrategy : ISyncStrategy
{
    private readonly IBulkCopyService _bulkCopyService;

    public DirectBulkCopyStrategy(IBulkCopyService bulkCopyService)
    {
        _bulkCopyService = bulkCopyService;
    }

    public async Task ExecuteAsync<T>(IAsyncEnumerable<T> syncData, DataSyncOptions options, ILogger logger) where T : class
    {
        try
        {
            await _bulkCopyService.CopyAsync<T>(syncData, options.TargetTable ?? string.Empty, options.ColumnMappings);
            logger.LogDebug("Direct bulk copy completed for {EntityType} to table {TargetTable}", typeof(T).Name, options.TargetTable);
        }
        catch (DataSyncException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new SyncStrategyException($"Direct bulk copy failed for {typeof(T).Name} to table {options.TargetTable}", ex);
        }
    }
}
