using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PseRestApi.Domain.Entities;

namespace PseRestApi.Core.Services.DataSync.HistoricalTradingDataSync;

public class HistoricalTradingDataSyncService : BaseSyncService<HistoricalTradingData>, IHistoricalTradingDataSyncService
{
    private readonly IServiceProvider _serviceProvider;

    public HistoricalTradingDataSyncService(
        IHistoricalTradingDataSyncDataProvider syncDataProvider,
        ISyncStrategyFactory strategyFactory,
        IServiceProvider serviceProvider,
        ILogger<HistoricalTradingDataSyncService> logger)
        : base(syncDataProvider, strategyFactory, logger)
    {
        _serviceProvider = serviceProvider;
    }

    public override Task Sync()
    {
        var provider = _serviceProvider.GetKeyedService<ISyncOptionsProvider>("HistoricalTradingData");
        return provider == null
            ? throw new InvalidOperationException("No ISyncOptionsProvider registered for HistoricalTradingData.")
            : SyncAsync(provider.GetOptions());
    }
}
