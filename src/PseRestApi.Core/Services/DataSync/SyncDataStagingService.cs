using PseRestApi.Domain.Entities;
using System.Text.Json;

namespace PseRestApi.Core.Services.DataSync;

public class SyncDataStagingService : ISyncDataStagingService
{
    private readonly IBulkCopyService _bulkCopyService;
    public SyncDataStagingService(
        IBulkCopyService bulkCopyService)
    {
        _bulkCopyService = bulkCopyService;
    }

    public async Task<Guid> Stage<T>(IAsyncEnumerable<T> stagingData, Guid? batchId = null) where T : class
    {
        var safeBatchId = (batchId == null || batchId == Guid.Empty) ? Guid.NewGuid() : batchId.GetValueOrDefault();
        var timeNow = DateTime.UtcNow;
        var syncBatchItems = new List<SyncBatchData>();

        try
        {
            await foreach (var item in stagingData)
            {
                var jsonValue = JsonSerializer.Serialize(item);
                var batchItem = new SyncBatchData()
                {
                    BatchId = safeBatchId,
                    Data = jsonValue,
                    Created = timeNow,
                };
                syncBatchItems.Add(batchItem);
            }
        }
        catch (JsonException ex)
        {
            throw new StagingException($"Serialization failed for {typeof(T).Name}", ex);
        }
        catch (Exception ex)
        {
            throw new StagingException($"Data enumeration failed for {typeof(T).Name}", ex);
        }

        var columnMappings = new Dictionary<string, string>
        {
            { "BatchId", "BatchId" },
            { "Data", "Data" },
            { "Created", "Created" }
        };

        try
        {
            await _bulkCopyService.CopyAsync(syncBatchItems.ToAsyncEnumerable(), typeof(SyncBatchData).Name, columnMappings);
        }
        catch (DataSyncException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new StagingException($"Bulk insert to staging failed for {typeof(T).Name}", ex);
        }

        return safeBatchId;
    }
}
