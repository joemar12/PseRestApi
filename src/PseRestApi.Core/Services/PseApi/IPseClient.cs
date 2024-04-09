using PseRestApi.Core.ResponseModels;

namespace PseRestApi.Core.Services.PseApi;

public interface IPseClient
{
    /// <summary>
    /// Get current stocks data from PSE
    /// </summary>
    /// <returns>List of all stocks with current prices</returns>
    Task<IEnumerable<StockFromFrames>> GetStocks();
    Task<StockCompanyResponse> FindSecurityOrCompany(string symbol);

    Task<IEnumerable<StockSummaryResponse>> GetAllStockSummary();

    Task<StockHeaderResponse> GetStockHeader(int companyId, int securityId);
}
