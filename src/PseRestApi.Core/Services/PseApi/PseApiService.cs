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

    public async Task<Stock?> GetStockPriceAsOfDateAsync(string symbol, DateOnly? asOfDate)
    {
        var historicalTradingData = await _appDbContext.HistoricalTradingData
            .AsNoTracking()
            .Include(x => x.SecurityInfo)
            .Where(x => x.Symbol == symbol && x.TradeDate <= asOfDate)
            .OrderByDescending(x => x.TradeDate)
            .ThenByDescending(x => x.Created)
            .FirstOrDefaultAsync();

        var stock = historicalTradingData == null ? null : Mappers.ManualMapper.MapToStock(historicalTradingData);
        return stock;
    }

    public async Task<Stock?> GetStockLatestPriceAsync(string symbol)
    {
        Stock? result = null;
        var stockData = await _client.GetStocks();
        var stock = stockData.FirstOrDefault(x => x.StockSymbol == symbol);
        if (stock != null)
        {
            result = Mappers.ManualMapper.MapToStock(stock);
        }
        return result;
    }

    public async Task<PaginatedResult<Stock>> GetStockPriceHistoryByDateRangeAsync(string stockSymbol, StockPriceQueryParams queryParams)
    {
        var pageNumber = queryParams.PageNumber ?? 1;
        var pageSize = queryParams.PageSize ?? 10;

        var query = _appDbContext.HistoricalTradingData
            .AsNoTracking()
            .Include(x => x.SecurityInfo)
            .Where(h => h.Symbol == stockSymbol
                && h.TradeDate != null
                && h.TradeDate >= queryParams.StartDate
                && (queryParams.EndDate == null || h.TradeDate <= queryParams.EndDate))
            .GroupBy(x => x.TradeDate)
            .Select(g => g.OrderByDescending(x => x.TradeDate).FirstOrDefault())
            .Where(x => x != null);

        var pageItems = await query
            .OrderBy(x => x.TradeDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new Stock
            {
                SecurityName = x.SecurityInfo == null ? "" : x.SecurityInfo.SecurityName,
                PercentChange = x.PercentChange ?? 0,
                Volume = x.Volume ?? 0,
                AsOfDate = x.TradeDate,
                Symbol = x.Symbol,
                Price = new List<StockPrice>
                {
                    new StockPrice { Currency = x.Currency, Price = x.Price ?? 0 }
                }
            })
            .ToListAsync();

        var totalCount = await query.CountAsync();

        return new PaginatedResult<Stock>
        {
            Items = pageItems,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}