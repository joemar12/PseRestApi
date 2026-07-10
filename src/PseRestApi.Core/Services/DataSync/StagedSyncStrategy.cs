using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using PseRestApi.Core.Common;

namespace PseRestApi.Core.Services.DataSync;

public class StagedSyncStrategy : ISyncStrategy
{
    private readonly ISyncDataStagingService _stagingService;
    private readonly IDbConnectionProvider _connectionProvider;

    public StagedSyncStrategy(
        ISyncDataStagingService stagingService,
        IDbConnectionProvider connectionProvider)
    {
        _stagingService = stagingService;
        _connectionProvider = connectionProvider;
    }

    public async Task ExecuteAsync<T>(IAsyncEnumerable<T> syncData, DataSyncOptions options, ILogger logger) where T : class
    {
        var batchId = await _stagingService.Stage(syncData, options.BatchId == Guid.Empty ? null : options.BatchId);

        using var connection = _connectionProvider.CreateConnection();
        var npgsqlConnection = connection as NpgsqlConnection
            ?? throw new InvalidOperationException("Connection is not an NpgsqlConnection");

        if (!string.IsNullOrEmpty(options.MergeCommand))
        {
            try
            {
                await npgsqlConnection.ExecuteAsync(options.MergeCommand, new { BatchId = batchId });
                logger.LogDebug("Merge command executed successfully for {EntityType}", typeof(T).Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Merge command failed for {EntityType}", typeof(T).Name);
                throw;
            }
        }

        if (!string.IsNullOrEmpty(options.CleanupCommand))
        {
            try
            {
                await npgsqlConnection.ExecuteAsync(options.CleanupCommand, new { BatchId = batchId });
                logger.LogDebug("Cleanup command executed successfully for {EntityType}", typeof(T).Name);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Cleanup command failed for {EntityType}", typeof(T).Name);
            }
        }
    }
}
