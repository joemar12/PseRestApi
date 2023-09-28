using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PseRestApi.Core.Common.Interfaces;
using PseRestApi.Core.Services.PseApi;
using PseRestApi.Domain.Entities;

namespace PseRestApi.Core.Services.DataSync.HistoricalTradingDataSync;

public class HistoricalTradingDataSyncDataProvider : IHistoricalTradingDataSyncDataProvider
{
    private readonly IPseClient _pseClient;
    private readonly IMapper _mapper;
    private readonly IAppDbContext _appDbContext;

    public HistoricalTradingDataSyncDataProvider(IPseClient pseClient, IMapper mapper, IAppDbContext appDbContext)
    {
        _pseClient = pseClient;
        _mapper = mapper;
        _appDbContext = appDbContext;
    }

    public async IAsyncEnumerable<HistoricalTradingData> GetSyncData()
    {
        //eagerly fetch all security info
        var allSecurityInfo = await _appDbContext.SecurityInfo.ToListAsync();
        var allStocksFromFrames = await _pseClient.GetStocks();
        var timeNow = DateTime.Now;

        foreach (var securityInfo in allSecurityInfo)
        {
            if (!string.IsNullOrEmpty(securityInfo.Symbol))
            {
                var stock = allStocksFromFrames.Where(x => x.StockSymbol == securityInfo.Symbol).FirstOrDefault();
                if (stock != null)
                {
                    if (stock != null && 
                        double.TryParse(stock.Price, out double price) && 
                        price > 0)
                    {
                        var tradingData = _mapper.Map<HistoricalTradingData>(stock);
                        tradingData.Id = Guid.NewGuid();
                        tradingData.SecurityId = securityInfo.SecurityId;
                        tradingData.Created = timeNow;
                        tradingData.LastModified = timeNow;
                        yield return tradingData;
                    }
                }
            }
        }
    }
}