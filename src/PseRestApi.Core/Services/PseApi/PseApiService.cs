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

    public async Task<Stock> GetStockPriceAsOfDateAsync(string symbol, DateTime? asOfDate)
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

    public async Task<PaginatedResult<Stock>> GetStockPriceHistoryByDateRangeAsync(string stockSymbol, StockPriceQueryParams queryParams)
    {
        var pageNumber = queryParams.PageNumber ?? 1;
        var pageSize = queryParams.PageSize ?? 10;

        var query = _appDbContext.HistoricalTradingData
            .Include(x => x.SecurityInfo)
            .Where(h => h.Symbol == stockSymbol
                && h.LastTradedDate != null
                && h.LastTradedDate >= queryParams.StartDate
                && (queryParams.EndDate == null || h.LastTradedDate <= queryParams.EndDate)
                && h.LastTradedDate == _appDbContext.HistoricalTradingData // using a subquery here since EF cannot properly translate grouping by LastTradedDate.Date
                    .Where(x => x.Symbol == stockSymbol
                             && x.LastTradedDate != null
                             && x.LastTradedDate.Value.Date == h.LastTradedDate!.Value.Date)
                    .Max(x => x.LastTradedDate));

        var pageItems = await query
            .OrderBy(x => x.LastTradedDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new Stock
            {
                SecurityName = x.SecurityInfo == null ? "" : x.SecurityInfo.SecurityName,
                PercentChange = x.PercChangeClose ?? 0,
                Volume = x.TotalVolume ?? 0,
                AsOfDate = x.LastTradedDate,
                Symbol = x.Symbol,
                Price = new List<StockPrice>
                {
                    new StockPrice { Currency = x.Currency, Price = x.LastTradePrice ?? 0 }
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