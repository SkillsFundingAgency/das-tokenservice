namespace SFA.DAS.TokenService.Infrastructure.Http;

public interface IHttpClientWrapper
{
    Task<T?> Post<T>(string? url, object content);
}