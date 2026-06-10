using PseRestApi.Core.ResponseModels;

namespace PseRestApi.Core.Services.PseApi;

public interface IPseClient
{
    Task<IEnumerable<StockFromFrames>> GetStocks();
}