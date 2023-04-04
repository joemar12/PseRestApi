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
            .WithTargetTable("dbo.HistoricalTradingData")
            .WithColumnMappings(new Dictionary<string, string>()
            {
                { "Id", "Id" },
                { "SecurityId", "SecurityId" },
                { "Symbol", "Symbol" },
                { "Currency", "Currency" },
                { "SqPrevious", "SqPrevious" },
                { "SqOpen", "SqOpen" },
                { "SqHigh", "SqHigh" },
                { "SqLow", "SqLow" },
                { "FiftyTwoWeekHigh", "FiftyTwoWeekHigh" },
                { "FiftyTwoWeekLow", "FiftyTwoWeekLow" },
                { "ChangeClose", "ChangeClose" },
                { "PercChangeClose", "PercChangeClose" },
                { "ChangeClosePercChangeClose", "ChangeClosePercChangeClose" },
                { "AvgPrice", "AvgPrice" },
                { "LastTradePrice", "LastTradePrice" },
                { "LastTradedDate", "LastTradedDate" },
                { "CurrentPe", "CurrentPe" },
                { "TotalValue", "TotalValue" },
                { "TotalVolume", "TotalVolume" },
                { "Created", "Created" },
                { "LastModified", "LastModified" }
            })
        );
    }
}
