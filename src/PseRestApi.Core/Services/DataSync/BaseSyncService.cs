using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using PseRestApi.Core.Common;

namespace PseRestApi.Core.Services.DataSync;

public abstract class BaseSyncService<T> : ISyncService where T : class
{
    private readonly ISyncDataProvider<T> _syncDataProvider;
    private readonly ISyncDataStagingService _syncDataStagingService;
    private readonly IDbConnectionProvider _connectionProvider;
    private readonly ILogger _logger;
    private DataSyncOptions _options = new DataSyncOptions();
    public BaseSyncService(
        ISyncDataProvider<T> syncDataProvider,
        ISyncDataStagingService syncDataStagingService,
        IDbConnectionProvider connectionProvider,
        ILogger logger)
    {
        _syncDataProvider = syncDataProvider;
        _connectionProvider = connectionProvider;
        _syncDataStagingService = syncDataStagingService;
        _logger = logger;
    }
    public abstract Task Sync();
    protected async Task Sync(Action<DataSyncOptionsBuilder> configureOptions)
    {
        var optionsBuilder = new DataSyncOptionsBuilder();
        configureOptions(optionsBuilder);
        _options = optionsBuilder.Build();

        var syncData = _syncDataProvider.GetSyncData();
        if (!_options.SkipStaging)
        {
            var batchId = await _syncDataStagingService.Stage(syncData, _options.BatchId);
            using var connection = _connectionProvider.CreateConnection();
            if (!string.IsNullOrEmpty(_options.MergeCommand))
            {
                try
                {
                    await connection.ExecuteAsync(_options.MergeCommand, new { BatchId = batchId });
                    if (!string.IsNullOrEmpty(_options.CleanupCommand))
                    {
                        await connection.ExecuteAsync(_options.CleanupCommand, new { BatchId = batchId });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Data sync failed for {nameof(T)}.");
                    throw;
                }
            }
        }
        else
        {
            //directly save data to target table
            var bulkInsertData = await syncData.ToListAsync();
            using var connection = _connectionProvider.CreateConnection();
            if (connection != null)
            {
                using var bulkCopier = new SqlBulkCopy(connection as SqlConnection);
                var sourceTable = bulkInsertData.ToDataTable();
                bulkCopier.DestinationTableName = _options.TargetTable;
                try
                {
                    if (_options.ColumnMappings != null && _options.ColumnMappings.Count > 0)
                    {
                        foreach (var mapping in _options.ColumnMappings)
                        {
                            bulkCopier.ColumnMappings.Add(mapping.Key, mapping.Value);
                        }
                    }
                    connection.Open();
                    await bulkCopier.WriteToServerAsync(sourceTable);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Data staging failed for {nameof(T)}");
                    throw;
                }
            }
        }
    }
}