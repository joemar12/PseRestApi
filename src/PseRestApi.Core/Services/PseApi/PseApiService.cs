using Microsoft.EntityFrameworkCore;
using PseRestApi.Core.Common.Interfaces;
using PseRestApi.Core.Dto;

namespace PseRestApi.Core.Services.PseApi;

public class PseApiService : IPseApiService
{

    private readonly IPseClient _client;
    private readonly IAppDbContext _appDbContext;

    public PseApiService(IPseClient client, IAppDbContext appDbContext)
    {
        _client = client;
        _appDbContext = appDbContext;
    }

    public async Task<Stock> GetHistoricalPrice(string symbol, DateTime? asOfDate)
    {
        var historicalTradingData = await _appDbContext.HistoricalTradingData
            .Include(x => x.SecurityInfo)
            .Where(x => x.Symbol == symbol && x.LastTradedDate <= asOfDate)
            .OrderByDescending(x => x.LastTradedDate)
            .ThenByDescending(x => x.Created)
            .FirstOrDefaultAsync();

        var stock = historicalTradingData == null ? new Stock() : Mappers.ManualMapper.MapToStock(historicalTradingData);
        return stock;
    }

    public async Task<Stock> GetStockLatestPrice(string symbol)
    {
        var result = new Stock();
        var stockData = await _client.GetStocks();
        var stock = stockData.FirstOrDefault(x => x.StockSymbol == symbol);
        if (stock != null)
        {
            result = Mappers.ManualMapper.MapToStock(stock);
        }
        return result;
    }
}