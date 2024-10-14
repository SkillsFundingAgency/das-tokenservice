namespace SFA.DAS.TokenService.Api.Client;

public interface ISecureHttpClient
{
    Task<string> GetAsync(string url);
}