namespace Fubaza.Application.Core.Contracts.Services
{
    public interface ICacheService
    {
        Task<T> GetOrSetAsync<T>(string cacheKey, Func<Task<T>> getData);
        void Remove(string cacheKey);
    }
}
