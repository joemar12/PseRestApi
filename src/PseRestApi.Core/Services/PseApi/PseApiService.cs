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

        var baseFilter = _appDbContext.HistoricalTradingData
            .AsNoTracking()
            .Where(h => h.Symbol == stockSymbol
                && h.TradeDate != null
                && h.TradeDate >= queryParams.StartDate
                && (queryParams.EndDate == null || h.TradeDate <= queryParams.EndDate));

        // Step 1: latest Created timestamp per TradeDate
        var latestCreatedPerDate = baseFilter
            .GroupBy(h => h.TradeDate)
            .Select(g => new
            {
                TradeDate = g.Key,
                LatestCreated = g.Max(x => x.Created)
            });

        // Step 2: among rows sharing that (TradeDate, Created) pair, pick the max Id
        var latestPerDate =
            from h in baseFilter
            join l in latestCreatedPerDate
                on new { h.TradeDate, Created = h.Created } equals new { l.TradeDate, Created = l.LatestCreated }
            group h by new { h.TradeDate, h.Created } into g
            select new
            {
                g.Key.TradeDate,
                g.Key.Created,
                LatestId = g.Max(x => x.Id)
            };

        // Step 3: join back to get the actual full row for each (TradeDate, Created, Id) triple
        var query =
            from h in _appDbContext.HistoricalTradingData.AsNoTracking()
            join l in latestPerDate
                on new { h.TradeDate, h.Created, h.Id } equals new { l.TradeDate, l.Created, Id = l.LatestId }
            select new
            {
                Historical = h,
                Security = h.SecurityInfo
            };

        var pageItems = await query
            .OrderBy(x => x.Historical.TradeDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new Stock
            {
                SecurityName = x.Security == null ? "" : x.Security.SecurityName,
                PercentChange = x.Historical.PercentChange ?? 0,
                Volume = x.Historical.Volume ?? 0,
                AsOfDate = x.Historical.TradeDate,
                Symbol = x.Historical.Symbol,
                Price = new List<StockPrice>
                {
            new StockPrice { Currency = x.Historical.Currency, Price = x.Historical.Price ?? 0 }
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