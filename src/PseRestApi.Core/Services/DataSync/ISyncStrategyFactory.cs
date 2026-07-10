namespace PseRestApi.Core.Services.DataSync;

public interface ISyncStrategyFactory
{
    ISyncStrategy GetStrategy(DataSyncOptions options);
}
