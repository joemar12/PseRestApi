using PseRestApi.Core.Services.PseApi;
using PseRestApi.Domain.Entities;

namespace PseRestApi.Core.Services.DataSync.SecurityInfoSync;

public class SecurityInfoSyncDataProvider : ISecurityInfoSyncDataProvider
{
    private readonly IPseClient _pseClient;

    public SecurityInfoSyncDataProvider(IPseClient pseClient)
    {
        _pseClient = pseClient;
    }

    public async IAsyncEnumerable<SecurityInfo> GetSyncData()
    {
        var allStocks = await _pseClient.GetStocks();
        foreach (var stock in allStocks)
        {
            if (!string.IsNullOrEmpty(stock.StockSymbol))
            {
                var secInfo = Mappers.ManualMapper.MapToSecurityInfo(stock);
                yield return secInfo;
            }
        }
    }
}
