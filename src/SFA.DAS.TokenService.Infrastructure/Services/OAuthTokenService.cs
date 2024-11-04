using SFA.DAS.TokenService.Domain;
using SFA.DAS.TokenService.Domain.Services;
using SFA.DAS.TokenService.Infrastructure.Configuration;
using SFA.DAS.TokenService.Infrastructure.Http;

namespace SFA.DAS.TokenService.Infrastructure.Services;

public class OAuthTokenService(
    IHttpClientWrapper httpClient,
    OAuthTokenServiceConfiguration configuration)
    : IOAuthTokenService
{
    public async Task<OAuthAccessToken> GetAccessToken(string privilegedAccessToken)
    {
        var clientSecret = GetClientSecret();

        var request = new OAuthTokenRequest(
            configuration.ClientId,
            $"{privilegedAccessToken}{clientSecret}"
        );

        return await GetToken(request);
    }

    public async Task<OAuthAccessToken> GetAccessToken(string privilegedAccessToken, string refreshToken)
    {
        var clientSecret = GetClientSecret();

        var request = new OAuthTokenRequest(
            configuration.ClientId,
            $"{privilegedAccessToken}{clientSecret}",
            refreshToken
        );

        return await GetToken(request);
    }

    private string GetClientSecret()
    {
        return configuration.ClientSecret == "." ? string.Empty : configuration.ClientSecret;
    }

    private async Task<OAuthAccessToken> GetToken(OAuthTokenRequest request)
    {
        var hmrcToken = await httpClient.Post<OAuthTokenResponse>(configuration.Url, request);

        return new OAuthAccessToken
        {
            AccessToken = hmrcToken!.AccessToken,
            RefreshToken = hmrcToken.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddSeconds(hmrcToken.ExpiresIn),
            Scope = hmrcToken.Scope,
            TokenType = hmrcToken.TokenType
        };
    }
}