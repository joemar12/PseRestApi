using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using PseRestApi.Core.Common;
using PseRestApi.Domain.Entities;
using System.Text.Json;

namespace PseRestApi.Core.Services.DataSync;

public class SyncDataStagingService : ISyncDataStagingService
{
    private readonly IDbConnectionProvider _connectionProvider;
    private readonly ILogger<SyncDataStagingService> _logger;
    public SyncDataStagingService(IDbConnectionProvider dbConnectionProvider, ILogger<SyncDataStagingService> logger)
    {
        _connectionProvider = dbConnectionProvider;
        _logger = logger;
    }
    public async Task<Guid> Stage<T>(IAsyncEnumerable<T> stagingData) where T : class
    {
        var batchId = Guid.NewGuid();
        var timeNow = DateTime.Now;
        var syncBatchItems = new List<SyncBatchData>();
        await foreach (var item in stagingData)
        {
            var jsonValue = JsonSerializer.Serialize(item);
            var batchItem = new SyncBatchData()
            {
                Id = Guid.NewGuid(),
                BatchId = batchId,
                Data = jsonValue,
                Created = timeNow,
                LastModified = timeNow,
            };
            syncBatchItems.Add(batchItem);
        }

        using var connection = _connectionProvider.CreateConnection();
        if (connection != null)
        {
            using var bulkCopier = new SqlBulkCopy(connection as SqlConnection);
            var sourceTable = syncBatchItems.ToDataTable();
            bulkCopier.DestinationTableName = "dbo.SyncBatchData";
            try
            {
                connection.Open();
                await bulkCopier.WriteToServerAsync(sourceTable);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Data staging failed for {nameof(T)}");
                throw;
            }
        }
        return batchId;
    }
}
