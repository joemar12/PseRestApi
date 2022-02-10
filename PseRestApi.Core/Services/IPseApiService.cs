using PseRestApi.Core.Dto;

namespace PseRestApi.Core.Services
{
    public interface IPseApiService
    {
        Task<Stock> GetStockLatestPrice(string symbol);
    }
}