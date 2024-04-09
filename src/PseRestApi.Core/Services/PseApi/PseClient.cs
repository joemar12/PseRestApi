using Flurl.Http;
using Flurl.Http.Configuration;
using HtmlAgilityPack;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using PseRestApi.Core.ResponseModels;
using System.Text.Json;

namespace PseRestApi.Core.Services.PseApi;

public class PseClient : IPseClient
{
    private readonly IFlurlClient _client;
    private readonly IFlurlClient _framesClient;
    private readonly ICacheProvider _cacheProvider;
    private const string CACHE_KEY = "stocks_from_frames";
    private readonly SemaphoreSlim _semaphore;

    public PseClient(IFlurlClientFactory clientFactory, IOptions<PseApiOptions> options, ICacheProvider cacheProvider)
    {
        IFlurlClient client = clientFactory.Get(options.Value.BaseUrl);
        client.Settings.BeforeCall = call =>
        {
            call.Request.WithHeader("Referer", options.Value.Referer);
        };
        _client = client;
        _framesClient = clientFactory.Get(options.Value.FramesUrl);
        _cacheProvider = cacheProvider;
        _semaphore = new SemaphoreSlim(1, 1);
    }

    private async Task<string> GetPseFramesJsonString()
    {
        var framesResponse = await _framesClient.Request().GetStringAsync();
        var framesHtmlDoc = new HtmlDocument();
        framesHtmlDoc.LoadHtml(framesResponse);
        var stocksInputElement = framesHtmlDoc.GetElementbyId("JsonId");
        if (stocksInputElement != null)
        {
            return stocksInputElement.Attributes["value"].Value;
        }
        return string.Empty;
    }

    public async Task<IEnumerable<StockFromFrames>> GetStocks()
    {
        try
        {
            await _semaphore.WaitAsync();
            var cachedData = _cacheProvider.GetFromCache<IEnumerable<StockFromFrames>>(CACHE_KEY);
            if (cachedData != null)
            {
                return cachedData;
            }
            else
            {
                var stocksJsonString = await GetPseFramesJsonString();
                stocksJsonString = stocksJsonString
                    .Replace("&quot;", "\"")
                    .Replace("&#x2B", "+");
                IEnumerable<StockFromFrames>? pseFramesResponse = new List<StockFromFrames>();
                if (!string.IsNullOrEmpty(stocksJsonString))
                {
                    pseFramesResponse = JsonSerializer.Deserialize<IEnumerable<StockFromFrames>>(stocksJsonString);
                }
                _cacheProvider.SetCache(CACHE_KEY, pseFramesResponse, new MemoryCacheEntryOptions()
                {
                    AbsoluteExpiration = DateTime.Now.AddHours(1),
                    SlidingExpiration = TimeSpan.FromMinutes(10),
                    Priority = CacheItemPriority.High
                });
                return pseFramesResponse ?? new List<StockFromFrames>();
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<StockCompanyResponse> FindSecurityOrCompany(string symbol)
    {
        var queryString = new
        {
            method = "findSecurityOrCompany",
            ajax = true,
            start = 0,
            limit = 1,
            query = symbol
        };
        return await _client.Request()
            .AppendPathSegment($"stockMarket/home.html")
            .SetQueryParams(queryString)
            .GetJsonAsync<StockCompanyResponse>();
    }

    public async Task<IEnumerable<StockSummaryResponse>> GetAllStockSummary()
    {
        var queryString = new
        {
            method = "getSecuritiesAndIndicesForPublic",
            ajax = true
        };
        return await _client.Request()
            .AppendPathSegment($"stockMarket/home.html")
            .SetQueryParams(queryString)
            .GetJsonAsync<IEnumerable<StockSummaryResponse>>();
    }

    public async Task<StockHeaderResponse> GetStockHeader(int companyId, int securityId)
    {
        var queryString = new
        {
            method = "fetchHeaderData",
            ajax = true,
            company = companyId,
            security = securityId
        };
        return await _client.Request()
            .AppendPathSegment($"stockMarket/companyInfo.html")
            .SetQueryParams(queryString)
            .GetJsonAsync<StockHeaderResponse>();
    }
}
