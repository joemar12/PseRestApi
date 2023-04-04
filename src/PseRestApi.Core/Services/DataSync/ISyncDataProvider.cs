namespace PseRestApi.Core.Services.DataSync;

public interface ISyncDataProvider<TData>
{
    public IAsyncEnumerable<TData> GetSyncData();
}