using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PseRestApi.Domain.Entities;

namespace PseRestApi.Core.Services.DataSync.SecurityInfoSync;

public class SecurityInfoDataSyncService : BaseSyncService<SecurityInfo>, ISecurityInfoDataSyncService
{
    private readonly IServiceProvider _serviceProvider;

    public SecurityInfoDataSyncService(
        ISecurityInfoSyncDataProvider syncDataProvider,
        ISyncStrategyFactory strategyFactory,
        IServiceProvider serviceProvider,
        ILogger<SecurityInfoDataSyncService> logger)
        : base(syncDataProvider, strategyFactory, logger)
    {
        _serviceProvider = serviceProvider;
    }

    public override Task Sync()
    {
        var provider = _serviceProvider.GetKeyedService<ISyncOptionsProvider>("SecurityInfo");
        return provider == null
            ? throw new InvalidOperationException("No ISyncOptionsProvider registered for SecurityInfo.")
            : SyncAsync(provider.GetOptions());
    }
}
