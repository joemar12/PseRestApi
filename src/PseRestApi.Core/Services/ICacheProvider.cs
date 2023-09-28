using Microsoft.Extensions.Caching.Memory;

namespace PseRestApi.Core.Services;
public interface ICacheProvider
{
    T GetFromCache<T>(string key);
    void SetCache<T>(string key, T value);
    void SetCache<T>(string key, T value, DateTimeOffset duration);
    void SetCache<T>(string key, T value, MemoryCacheEntryOptions options);
    void ClearCache(string key);
}
