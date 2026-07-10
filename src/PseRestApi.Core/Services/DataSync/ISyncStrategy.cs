using Microsoft.Extensions.Logging;

namespace PseRestApi.Core.Services.DataSync;

public interface ISyncStrategy
{
    Task ExecuteAsync<T>(IAsyncEnumerable<T> syncData, DataSyncOptions options, ILogger logger) where T : class;
}
