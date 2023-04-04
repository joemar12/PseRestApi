using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PseRestApi.Core.Common.Interfaces;
using PseRestApi.Core.Dto;

namespace PseRestApi.Core.Services.Pse;

public class PseApiService : IPseApiService
{
    private readonly IMapper _mapper;
    private readonly IPseClient _client;
    private readonly IAppDbContext _appDbContext;

    public PseApiService(IMapper mapper, IPseClient client, IAppDbContext appDbContext)
    {
        _mapper = mapper;
        _client = client;
        _appDbContext = appDbContext;
    }

    public async Task<Stock> GetHistoricalPrice(string symbol, DateTime? asOfDate)
    {
        var historicalTradingData = await _appDbContext.HistoricalTradingData
            .Include(x => x.SecurityInfo)
            .Where(x => x.Symbol == symbol && x.LastTradedDate <= asOfDate)
            .OrderByDescending(x => x.LastTradedDate)
            .FirstOrDefaultAsync();

        var stock = _mapper.Map<Stock>(historicalTradingData);
        return stock;
    }

    public async Task<Stock> GetStockLatestPrice(string symbol)
    {
        var result = new Stock();
        var stockCompany = await _client.FindSecurityOrCompany(symbol);
        if (stockCompany != null && stockCompany.Records != null)
        {
            var companyDetail = stockCompany.Records.FirstOrDefault();
            if (companyDetail != null)
            {
                var stockHeader = await _client.GetStockHeader(companyDetail.CompanyId, companyDetail.SecurityId);
                if (stockHeader != null)
                {
                    _mapper.Map(stockHeader, result);
                    _mapper.Map(companyDetail, result);
                }
            }
        }

        return result;
    }
}