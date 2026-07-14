namespace PseRestApi.Core.Services.DataSync;

public interface IBulkCopyService
{
    Task CopyAsync<T>(string targetTable, IAsyncEnumerable<T> data, DataSyncOptions? options = null)
        where T : class;
}
