namespace PseRestApi.Core.Services.DataSync;

public interface IBulkCopyService
{
    Task CopyAsync<T>(IAsyncEnumerable<T> data, string targetTable, IDictionary<string, string>? columnMappings = null)
        where T : class;
}
