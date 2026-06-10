using HtmlAgilityPack;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using PseRestApi.Core.ResponseModels;
using System.Text.Json;

namespace PseRestApi.Core.Services.PseApi;

public class PseClient : IPseClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ICacheProvider _cacheProvider;
    private readonly SemaphoreSlim _semaphore;

    public PseClient(IOptions<PseApiOptions> options, ICacheProvider cacheProvider, IHttpClientFactory httpClientFactory)
    {
        _cacheProvider = cacheProvider;
        Options = options?.Value ?? new PseApiOptions();
        _semaphore = new SemaphoreSlim(1, 1);
        _httpClientFactory = httpClientFactory;
    }

    private PseApiOptions Options { get; }

    private async Task<string> GetPseFramesJsonString()
    {
        var client = _httpClientFactory.CreateClient(Constants.PseFramesClientName);
        using var req = new HttpRequestMessage(HttpMethod.Get, string.Empty);
        if (!string.IsNullOrEmpty(Options.Referer))
        {
            if (Uri.TryCreate(Options.Referer, UriKind.Absolute, out var refUri))
            {
                req.Headers.Referrer = refUri;
            }
            else
            {
                req.Headers.Add("Referer", Options.Referer);
            }
        }
        using var resp = await client.SendAsync(req);
        resp.EnsureSuccessStatusCode();
        var framesResponse = await resp.Content.ReadAsStringAsync();
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
            var cachedData = _cacheProvider.GetFromCache<IEnumerable<StockFromFrames>>(Constants.StocksCacheKey);
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
                _cacheProvider.SetCache(Constants.StocksCacheKey, pseFramesResponse, new MemoryCacheEntryOptions()
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
}
