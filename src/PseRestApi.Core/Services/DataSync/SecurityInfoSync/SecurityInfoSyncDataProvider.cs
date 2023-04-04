using AutoMapper;
using PseRestApi.Core.Services.Pse;
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
        var allStocks = await _pseClient.GetAllStockSummary();
        foreach (var stockSummary in allStocks)
        {
            if (!string.IsNullOrEmpty(stockSummary.SecuritySymbol))
            {
                var apiResponse = await _pseClient.FindSecurityOrCompany(stockSummary.SecuritySymbol);
                if (apiResponse != null && apiResponse.Records != null)
                {
                    var secInfoFromApi = apiResponse.Records.FirstOrDefault();
                    if (secInfoFromApi != null && !string.IsNullOrEmpty(secInfoFromApi.Symbol))
                    {
                        var secInfo = _mapper.Map<SecurityInfo>(secInfoFromApi);
                        yield return secInfo;
                    }
                }
            }
        }
    }
}
