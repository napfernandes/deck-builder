using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;

namespace DeckBuilder.Api.Helpers;

public static class CacheManager
{
    private static readonly HashSet<string> CacheKeys = [];
    private static readonly IMemoryCache MemoryCache = new MemoryCache(new MemoryCacheOptions());

    public static void SetItem(string cacheKey, object value, TimeSpan? duration = null)
    {
        CacheKeys.Add(cacheKey);
        MemoryCache.Set(cacheKey, value, duration ?? TimeSpan.FromMinutes(30));
    }

    public static T? GetItem<T>(string cacheKey)
    {
        MemoryCache.TryGetValue(cacheKey, out T? cacheResult);
        return cacheResult;
    }

    public static void RemoveItem(string cacheKey)
    {
        var regex = new Regex($"^{cacheKey}");
        var allCacheKeys = CacheKeys.Where(key => regex.IsMatch(key)).ToList();
        allCacheKeys.ForEach(foundKey =>
        {
            MemoryCache.Remove(foundKey);
            CacheKeys.Remove(foundKey);
        });
    }

    public static void Clear()
    {
        foreach (var cacheKey in CacheKeys)
        {
            MemoryCache.Remove(cacheKey);
            CacheKeys.Remove(cacheKey);
        }
    }
}