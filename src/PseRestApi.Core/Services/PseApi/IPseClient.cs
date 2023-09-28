using PseRestApi.Core.ResponseModels;

namespace PseRestApi.Core.Services.PseApi;

public interface IPseClient
{
    Task<IEnumerable<StockFromFrames>> GetStocks();
    Task<StockCompanyResponse> FindSecurityOrCompany(string symbol);

    Task<IEnumerable<StockSummaryResponse>> GetAllStockSummary();

    Task<StockHeaderResponse> GetStockHeader(int companyId, int securityId);
}