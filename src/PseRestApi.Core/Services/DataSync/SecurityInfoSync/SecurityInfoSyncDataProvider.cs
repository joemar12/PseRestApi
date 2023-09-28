using AutoMapper;
using PseRestApi.Core.Services.PseApi;
using PseRestApi.Domain.Entities;

namespace PseRestApi.Core.Services.DataSync.SecurityInfoSync;

public class SecurityInfoSyncDataProvider : ISecurityInfoSyncDataProvider
{
    private readonly IPseClient _pseClient;
    private readonly IMapper _mapper;

    public SecurityInfoSyncDataProvider(IPseClient pseClient, IMapper mapper)
    {
        _pseClient = pseClient;
        _mapper = mapper;
    }

    public async IAsyncEnumerable<SecurityInfo> GetSyncData()
    {
        var allStocks = await _pseClient.GetStocks();
        foreach (var stock in allStocks)
        {
            if (!string.IsNullOrEmpty(stock.StockSymbol))
            {
                var secInfo = _mapper.Map<SecurityInfo>(stock);
                yield return secInfo;
            }
        }
    }
}
