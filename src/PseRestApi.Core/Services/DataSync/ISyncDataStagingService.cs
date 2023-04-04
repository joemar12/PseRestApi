namespace PseRestApi.Core.Services.DataSync;

public interface ISyncDataStagingService
{
    Task<Guid> Stage<T>(IAsyncEnumerable<T> stagingData) where T : class;
}
