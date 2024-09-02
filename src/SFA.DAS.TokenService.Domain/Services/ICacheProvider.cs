namespace SFA.DAS.TokenService.Domain.Services;

public interface ICacheProvider
{
    Task<object> GetAsync(string key);
    Task SetAsync(string key, object value, DateTimeOffset? expiryTime = null);
}