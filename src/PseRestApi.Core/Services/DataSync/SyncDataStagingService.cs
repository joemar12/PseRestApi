using Microsoft.Extensions.Logging;
using Npgsql;
using PseRestApi.Core.Common;
using PseRestApi.Domain.Entities;
using System.Data;
using System.Text.Json;

namespace PseRestApi.Core.Services.DataSync;

public class SyncDataStagingService : ISyncDataStagingService
{
    private readonly IDbConnectionProvider _connectionProvider;
    private readonly ILogger<SyncDataStagingService> _logger;
    public SyncDataStagingService(
        IDbConnectionProvider dbConnectionProvider,
        ILogger<SyncDataStagingService> logger)
    {
        _connectionProvider = dbConnectionProvider;
        _logger = logger;
    }

    public async Task<Guid> Stage<T>(IAsyncEnumerable<T> stagingData, Guid? batchId = null) where T : class
    {
        var safeBatchId = (batchId == null || batchId == Guid.Empty) ? Guid.NewGuid() : batchId.GetValueOrDefault();
        var timeNow = DateTime.Now;
        var syncBatchItems = new List<SyncBatchData>();
        await foreach (var item in stagingData)
        {
            var jsonValue = JsonSerializer.Serialize(item);
            var batchItem = new SyncBatchData()
            {
                BatchId = safeBatchId,
                Data = jsonValue,
                Created = timeNow,
                LastModified = timeNow,
            };
            syncBatchItems.Add(batchItem);
        }

        using var connection = _connectionProvider.CreateConnection();
        if (connection != null)
        {
            var sourceTable = syncBatchItems.ToDataTable();
            var columnNames = sourceTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();
            try
            {
                await using var npgsqlConnection = connection as NpgsqlConnection ?? throw new InvalidOperationException("Connection is not an NpgsqlConnection");
                await npgsqlConnection.OpenAsync();
                await using var copyWriter = await npgsqlConnection.BeginBinaryImportAsync("COPY SyncBatchData (Id, BatchId, Data, Created, LastModified) FROM STDIN (FORMAT BINARY)");
                foreach (DataRow row in sourceTable.Rows)
                {
                    await copyWriter.StartRowAsync();
                    await copyWriter.WriteAsync(row["Id"]);
                    await copyWriter.WriteAsync(row["BatchId"]);
                    await copyWriter.WriteAsync(row["Data"] ?? DBNull.Value);
                    await copyWriter.WriteAsync(row["Created"] ?? DBNull.Value);
                    await copyWriter.WriteAsync(row["LastModified"] ?? DBNull.Value);
                }
                await copyWriter.CompleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Data staging failed for {nameof(T)}");
                throw;
            }
        }
        return safeBatchId;
    }
}
