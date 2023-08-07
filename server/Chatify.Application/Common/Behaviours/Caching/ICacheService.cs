namespace Chatify.Application.Common.Behaviours.Caching;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    
    Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = default, CancellationToken cancellationToken = default);
}