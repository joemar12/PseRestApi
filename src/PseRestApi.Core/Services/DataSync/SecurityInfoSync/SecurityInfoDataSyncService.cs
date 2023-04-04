using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PseRestApi.Core.Common;

namespace PseRestApi.Core.Services.DataSync.SecurityInfoSync;

public class SecurityInfoDataSyncService : ISyncService
{
    private readonly ISecurityInfoSyncDataProvider _syncDataProvider;
    private readonly ISyncDataStagingService _syncDataStagingService;
    private readonly IDbConnectionProvider _connectionProvider;
    private readonly ILogger<SecurityInfoDataSyncService> _logger;
    public SecurityInfoDataSyncService(
        ISecurityInfoSyncDataProvider syncDataProvider,
        ISyncDataStagingService syncDataStagingService,
        IDbConnectionProvider connectionProvider,
        ILogger<SecurityInfoDataSyncService> logger)
    {
        _syncDataProvider = syncDataProvider;
        _syncDataStagingService = syncDataStagingService;
        _connectionProvider = connectionProvider;
        _logger = logger;
    }

    public async Task Sync()
    {
        var syncData = _syncDataProvider.GetSyncData();
        var batchId = await _syncDataStagingService.Stage(syncData);
        using var connection = _connectionProvider.CreateConnection();

        var sqlCommand = GetMergeCommand(batchId);
        try
        {
            await connection.ExecuteAsync(sqlCommand);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SecurityInfo sync failed.");
            throw;
        }
    }

    private string GetMergeCommand(Guid batchId)
    {
        var command = $"";
        return "";
    }
}