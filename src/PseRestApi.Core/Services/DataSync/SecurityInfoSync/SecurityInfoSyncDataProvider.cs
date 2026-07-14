using PseRestApi.Core.ResponseModels;
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
        StockFromFrames[] allStocks;

        try
        {
            allStocks = (await DataSyncRetryPolicy.ExecuteAsync(
                async () => await _pseClient.GetStocks(),
                "fetch stocks from PSE API")).ToArray();
        }
        catch (DataProviderException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new DataProviderException("Failed to fetch security info", ex);
        }

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
