using AutoMapper;
using PseRestApi.Core.Dto;

namespace PseRestApi.Core.Services;

public class PseApiService : IPseApiService
{
    private readonly IMapper _mapper;
    private readonly IPseClient _client;

    public PseApiService(IMapper mapper, IPseClient client)
    {
        _mapper = mapper;
        _client = client;
    }

    public async Task<Stock> GetStockLatestPrice(string symbol)
    {
        var result = new Stock();
        var allStocks = await _client.GetAllStockSummary();
        var stockCompany = await _client.FindSecurityOrCompany(symbol);
        if (stockCompany != null && stockCompany.records != null)
        {
            var companyDetail = stockCompany.records.FirstOrDefault();
            var stockSummary = allStocks.Where(x => x.securitySymbol == symbol).FirstOrDefault();
            if (stockSummary != null && companyDetail != null)
            {
                var stockHeader = await _client.GetStockHeader(companyDetail.listedCompany_companyId, companyDetail.securityId);
                if (stockHeader != null)
                {
                    _mapper.Map(stockHeader, result);
                    _mapper.Map(companyDetail, result);
                    _mapper.Map(stockSummary, result);
                }
            }
        }

        return result;
    }
}