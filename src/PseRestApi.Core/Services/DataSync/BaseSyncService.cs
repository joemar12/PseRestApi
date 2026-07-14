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

    public abstract Task RunAsync();

    protected async Task RunAsyncImpl(DataSyncOptions options)
    {
        var syncData = _syncDataProvider.GetSyncData();
        var strategy = _strategyFactory.GetStrategy(options);

        _logger.LogDebug("Starting sync for {EntityType} with strategy {StrategyType}",
            typeof(T).Name, strategy.GetType().Name);

        try
        {
            await strategy.ExecuteAsync<T>(syncData, options, _logger);
            _logger.LogDebug("Sync completed for {EntityType}", typeof(T).Name);
        }
        catch (DataSyncException ex)
        {
            _logger.LogError(ex, "Sync failed for {EntityType} with strategy {StrategyType}: {Message}",
                typeof(T).Name, strategy.GetType().Name, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected sync failure for {EntityType}", typeof(T).Name);
            throw new OrchestrationException($"Sync failed for {typeof(T).Name}", ex);
        }
    }
}