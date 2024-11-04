using Newtonsoft.Json;

namespace SFA.DAS.TokenService.Infrastructure.Services;

public class OAuthTokenRequest
{
    [JsonProperty("grant_type")] public string GrantType => "client_credentials";

    [JsonProperty("scopes")] public string Scopes => "read:apprenticeship-levy";

    [JsonProperty("client_secret")] public string ClientSecret { get; private set; }

    [JsonProperty("client_id")] public string ClientId { get; private set; }

    [JsonProperty("refresh_token")] public string? RefreshToken { get; private set; }

    public OAuthTokenRequest(string clientId, string clientSecret)
    {
        ClientId = clientId;
        ClientSecret = clientSecret;
        RefreshToken = string.Empty;
    }

    public OAuthTokenRequest(string clientId, string clientSecret, string refreshToken)
    {
        ClientId = clientId;
        ClientSecret = clientSecret;
        RefreshToken = refreshToken;
    }
}