using PseRestApi.Core.Dto;

namespace PseRestApi.Core.Services.Pse;

public interface IPseApiService
{
    Task<Stock> GetStockLatestPrice(string symbol);
    Task<Stock> GetHistoricalPrice(string symbol, DateTime? asOfDate);
}