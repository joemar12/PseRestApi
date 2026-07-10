namespace PseRestApi.Core.Services.DataSync.HistoricalTradingDataSync;

public class HistoricalTradingDataSyncOptionsProvider : ISyncOptionsProvider
{
    public DataSyncOptions GetOptions() => new DataSyncOptionsBuilder()
        .WithSkipStaging(true)
        .WithTargetTable("HistoricalTradingData")
        .WithColumnMappings(new Dictionary<string, string>
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
        .Build();
}
