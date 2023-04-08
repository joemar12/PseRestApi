using PseRestApi.Core.Dto;

namespace PseRestApi.Core.Services.PseApi;

public interface IPseApiService
{
    Task<Stock> GetStockLatestPrice(string symbol);
    Task<Stock> GetHistoricalPrice(string symbol, DateTime? asOfDate);
}