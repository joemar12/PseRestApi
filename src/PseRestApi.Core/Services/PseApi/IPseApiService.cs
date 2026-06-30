using PseRestApi.Core.Dto;

namespace PseRestApi.Core.Services.PseApi;

public interface IPseApiService
{
    /// <summary>
    /// Gets the latest stock price for the given symbol.
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    Task<Stock> GetStockLatestPriceAsync(string symbol);
    /// <summary>
    /// Gets the stock price for the given symbol as of the specified date.
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="asOfDate"></param>
    /// <returns></returns>
    Task<Stock> GetStockPriceAsOfDateAsync(string symbol, DateOnly? asOfDate);
    /// <summary>
    /// Gets the stock price history for the given symbol within the specified date range, only getting the latest trade price per day.
    /// </summary>
    /// <param name="stockSymbol"></param>
    /// <param name="queryParams"></param>
    /// <returns></returns>
    Task<PaginatedResult<Stock>> GetStockPriceHistoryByDateRangeAsync(string stockSymbol, StockPriceQueryParams queryParams);
}