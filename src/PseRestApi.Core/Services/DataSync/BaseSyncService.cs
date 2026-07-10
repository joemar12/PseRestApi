using Microsoft.Extensions.Logging;

namespace PseRestApi.Core.Services.DataSync;

public abstract class BaseSyncService<T> : ISyncService where T : class
{
    private readonly ISyncDataProvider<T> _syncDataProvider;
    private readonly ISyncStrategyFactory _strategyFactory;
    private readonly ILogger _logger;

    public BaseSyncService(
        ISyncDataProvider<T> syncDataProvider,
        ISyncStrategyFactory strategyFactory,
        ILogger logger)
    {
        _syncDataProvider = syncDataProvider;
        _strategyFactory = strategyFactory;
        _logger = logger;
    }

    public abstract Task Sync();

    protected async Task SyncAsync(DataSyncOptions options)
    {
        var syncData = _syncDataProvider.GetSyncData();
        var strategy = _strategyFactory.GetStrategy(options);

        _logger.LogDebug("Starting sync for {EntityType} with strategy {StrategyType}",
            typeof(T).Name, strategy.GetType().Name);

        await strategy.ExecuteAsync<T>(syncData, options, _logger);

        _logger.LogDebug("Sync completed for {EntityType}", typeof(T).Name);
    }
}