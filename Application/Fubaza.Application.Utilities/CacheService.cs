using Microsoft.Extensions.Caching.Memory;

using Fubaza.Application.Core.Contracts.Services;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;

    public CacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public async Task<T> GetOrSetAsync<T>(string cacheKey, Func<Task<T>> getData)
    {
        if (_cache.TryGetValue(cacheKey, out T? result) && result != null)
        {
            return result;
        }

        result = await getData();

        if (result != null)
        {
            var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(6)) // Set a maximum lifespan for the cache entry, regardless of usage
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30)); // Expire based on inactivity, resetting the timer with each access

            _cache.Set(cacheKey, result, cacheOptions);
        }

        return result;
    }

    public void Remove(string cacheKey)
    {
        _cache.Remove(cacheKey);
    }
}
