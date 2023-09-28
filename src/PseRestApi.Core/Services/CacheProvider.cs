using Microsoft.Extensions.Caching.Memory;

namespace PseRestApi.Core.Services;
public class CacheProvider : ICacheProvider
{
    private const int CacheSeconds = 10;
    private readonly IMemoryCache _cache;

    public CacheProvider(IMemoryCache cache)
    {
        _cache = cache;
    }

    public T GetFromCache<T>(string key)
    {
        _cache.TryGetValue(key, out T? cachedResponse);
        return cachedResponse!;
    }

    public void SetCache<T>(string key, T value)
    {
        SetCache(key, value, DateTimeOffset.Now.AddSeconds(CacheSeconds));
    }

    public void SetCache<T>(string key, T value, DateTimeOffset duration)
    {
        _cache.Set(key, value, duration);
    }

    public void SetCache<T>(string key, T value, MemoryCacheEntryOptions options)
    {
        _cache.Set(key, value, options);
    }

    public void ClearCache(string key)
    {
        _cache.Remove(key);
    }
}