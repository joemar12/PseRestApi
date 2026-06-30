using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using PseRestApi.Core.Common;
using System.Data;

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
                var sourceTable = bulkInsertData.ToDataTable();
                var columnNames = sourceTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();
                try
                {
                    await using var npgsqlConnection = connection as NpgsqlConnection ?? throw new InvalidOperationException("Connection is not an NpgsqlConnection");
                    await npgsqlConnection.OpenAsync();
                    var targetTable = _options.TargetTable?.Replace("dbo.", "");
                    await using var copyWriter = await npgsqlConnection.BeginBinaryImportAsync(
                        $"COPY {targetTable} ({string.Join(", ", columnNames)}) FROM STDIN (FORMAT BINARY)");
                    foreach (DataRow row in sourceTable.Rows)
                    {
                        await copyWriter.StartRowAsync();
                        foreach (var columnName in columnNames)
                        {
                            var value = row[columnName];
                            await copyWriter.WriteAsync(value == DBNull.Value ? null : value);
                        }
                    }
                    await copyWriter.CompleteAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Data sync failed for {nameof(T)}");
                    throw;
                }
            }
        }
    }
}