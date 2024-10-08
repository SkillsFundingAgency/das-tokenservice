using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.TokenService.Domain;
using SFA.DAS.TokenService.Domain.Services;
using SFA.DAS.TokenService.Infrastructure.Configuration;
using SFA.DAS.TokenService.Infrastructure.Http;

namespace SFA.DAS.TokenService.Infrastructure.Services;

public class OAuthTokenService : IOAuthTokenService
{
    private readonly IHttpClientWrapper _httpClient;
    private readonly OAuthTokenServiceConfiguration _configuration;
    private readonly ILogger<OAuthTokenService> _logger;

    public OAuthTokenService(IHttpClientWrapper httpClient, OAuthTokenServiceConfiguration configuration, ILogger<OAuthTokenService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        _httpClient.AcceptHeaders.Add(new MediaTypeWithQualityHeaderValue("application/vnd.hmrc.1.0+json"));
    }

    public async Task<OAuthAccessToken> GetAccessToken(string privilegedAccessToken)
    {
        var request = new OAuthTokenRequest
        {
            ClientId = _configuration.ClientId,
            ClientSecret = $"{privilegedAccessToken}{_configuration.ClientSecret}",
            GrantType = "client_credentials",
            Scopes = "read:apprenticeship-levy"
        };

        _logger.LogInformation("OAuthTokenService. OAuthTokenRequest token: {Token}", JsonConvert.SerializeObject(request));

        var hmrcToken = await _httpClient.Post<OAuthTokenResponse>(_configuration.Url, request);

        return new OAuthAccessToken
        {
            AccessToken = hmrcToken!.AccessToken,
            RefreshToken = hmrcToken.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddSeconds(hmrcToken.ExpiresIn),
            Scope = hmrcToken.Scope,
            TokenType = hmrcToken.TokenType
        };
    }

    public async Task<OAuthAccessToken> GetAccessTokenFromRefreshToken(string privilegedAccessToken, string refreshToken)
    {
        var request = new OAuthTokenRefreshRequest
        {
            ClientId = _configuration.ClientId,
            ClientSecret = $"{privilegedAccessToken}{_configuration.ClientSecret}",
            GrantType = "client_credentials",
            Scopes = "read:apprenticeship-levy",
            RefreshToken = refreshToken
        };

        var hmrcToken = await _httpClient.Post<OAuthTokenResponse>(_configuration.Url, request);

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