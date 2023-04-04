using PseRestApi.Core.ResponseModels;

namespace PseRestApi.Core.Services.Pse;

public interface IPseClient
{
    Task<StockCompanyResponse> FindSecurityOrCompany(string symbol);

    Task<IEnumerable<StockSummaryResponse>> GetAllStockSummary();

    Task<StockHeaderResponse> GetStockHeader(int companyId, int securityId);
}