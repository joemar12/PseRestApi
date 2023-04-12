using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PseRestApi.Core.Common.Interfaces;
using PseRestApi.Core.Services.PseApi;
using PseRestApi.Domain.Entities;

namespace PseRestApi.Core.Services.DataSync.HistoricalTradingDataSync;

public class HistoricalTradingDataSyncDataProvider : IHistoricalTradingDataSyncDataProvider
{
    private readonly IPseClient _pseClient;
    private readonly IMapper _mapper;
    private readonly IAppDbContext _appDbContext;

    public HistoricalTradingDataSyncDataProvider(IPseClient pseClient, IMapper mapper, IAppDbContext appDbContext)
    {
        _pseClient = pseClient;
        _mapper = mapper;
        _appDbContext = appDbContext;
    }

    public async IAsyncEnumerable<HistoricalTradingData> GetSyncData()
    {
        //eagerly fetch all security info
        var allSecurityInfo = await _appDbContext.SecurityInfo.ToListAsync();
        var timeNow = DateTime.Now;
        foreach (var securityInfo in allSecurityInfo)
        {
            if (!string.IsNullOrEmpty(securityInfo.Symbol))
            {
                var apiResponse = await _pseClient.GetStockHeader(securityInfo.CompanyId, securityInfo.SecurityId);
                if (apiResponse != null && apiResponse.Records != null)
                {
                    var latestStockTradeData = apiResponse.Records.FirstOrDefault();
                    if (latestStockTradeData != null &&
                        !string.IsNullOrEmpty(latestStockTradeData.Symbol) &&
                        latestStockTradeData.LastTradePrice > 0)
                    {
                        var tradingData = _mapper.Map<HistoricalTradingData>(latestStockTradeData);
                        tradingData.Id = Guid.NewGuid();
                        tradingData.SecurityId = securityInfo.SecurityId;
                        tradingData.Created = timeNow;
                        tradingData.LastModified = timeNow;
                        yield return tradingData;
                    }
                }
            }
        }
    }
}