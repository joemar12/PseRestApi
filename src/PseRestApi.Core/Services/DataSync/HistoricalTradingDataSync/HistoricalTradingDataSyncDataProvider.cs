using Microsoft.EntityFrameworkCore;
using PseRestApi.Core.Common.Interfaces;
using PseRestApi.Core.Services.PseApi;
using PseRestApi.Domain.Entities;
using PseRestApi.Core.ResponseModels;

namespace PseRestApi.Core.Services.DataSync.HistoricalTradingDataSync;

public class HistoricalTradingDataSyncDataProvider : IHistoricalTradingDataSyncDataProvider
{
    private readonly IPseClient _pseClient;
    private readonly IAppDbContext _appDbContext;

    public HistoricalTradingDataSyncDataProvider(IPseClient pseClient, IAppDbContext appDbContext)
    {
        _pseClient = pseClient;
        _appDbContext = appDbContext;
    }

    public async IAsyncEnumerable<HistoricalTradingData> GetSyncData()
    {
        // Fetch data outside of try/yield block
        SecurityInfo[] allSecurityInfo;
        IEnumerable<StockFromFrames> allStocksFromFrames;
        DateTime timeNow;

        try
        {
            // eagerly fetch all security info with retry
            allSecurityInfo = (await DataSyncRetryPolicy.ExecuteAsync(
                async () => await _appDbContext.SecurityInfo.ToListAsync(),
                "fetch security info")).ToArray();

            allStocksFromFrames = (await DataSyncRetryPolicy.ExecuteAsync(
                async () => await _pseClient.GetStocks(),
                "fetch stocks from PSE API")).ToArray();

            timeNow = DateTime.UtcNow;
        }
        catch (DataProviderException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new DataProviderException("Failed to fetch historical trading data", ex);
        }

        // Now yield outside of try block
        foreach (var securityInfo in allSecurityInfo)
        {
            if (!string.IsNullOrEmpty(securityInfo.Symbol))
            {
                var stock = allStocksFromFrames.FirstOrDefault(x => x.StockSymbol == securityInfo.Symbol);
                if (stock != null)
                {
                    if (stock != null &&
                        double.TryParse(stock.Price, out double price) &&
                        price > 0)
                    {
                        var tradingData = Mappers.ManualMapper.MapToHistoricalTradingData(stock);
                        tradingData.SecurityId = securityInfo.SecurityId;
                        tradingData.Created = timeNow;
                        yield return tradingData;
                    }
                }
            }
        }
    }
}