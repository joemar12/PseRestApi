using Microsoft.Extensions.Logging;
using PseRestApi.Core.Common;
using PseRestApi.Domain.Entities;

namespace PseRestApi.Core.Services.DataSync.HistoricalTradingDataSync;

public class HistoricalTradingDataSyncService : BaseSyncService<HistoricalTradingData>, IHistoricalTradingDataSyncService
{
    public HistoricalTradingDataSyncService(
        IHistoricalTradingDataSyncDataProvider syncDataProvider,
        ISyncDataStagingService syncDataStagingService,
        IDbConnectionProvider connectionProvider,
        ILogger<HistoricalTradingDataSyncService> logger) : base(syncDataProvider, syncDataStagingService, connectionProvider, logger)
    {
    }
    public override async Task Sync()
    {
        await Sync(options =>
            options
            .SkipStaging()
            .WithTargetTable("HistoricalTradingData")
            .WithColumnMappings(new Dictionary<string, string>()
            {
                { "SecurityId", "SecurityId" },
                { "Symbol", "Symbol" },
                { "Currency", "Currency" },
                { "Change", "Change" },
                { "PercentChange", "PercentChange" },
                { "Price", "Price" },
                { "TradeDate", "TradeDate" },
                { "Value", "Value" },
                { "Volume", "Volume" },
                { "Created", "Created" },
            })
        );
    }
}
