using Newtonsoft.Json;

namespace SFA.DAS.TokenService.Infrastructure.Services;

public class OAuthTokenRequest
{
    [JsonProperty("grant_type")] 
    public string GrantType => "client_credentials";
    
    [JsonProperty("scopes")] 
    public string Scopes => "read:apprenticeship-levy";

    [JsonProperty("client_secret")] 
    public string ClientSecret { get; private set; } = string.Empty;

    [JsonProperty("client_id")] 
    public string ClientId { get; private set; } = string.Empty;

    [JsonProperty("refresh_token")] 
    public string? RefreshToken { get; private set; } = string.Empty;

    public static OAuthTokenRequest Create(string clientId, string clientSecret)
    {
        return Create(clientId, clientSecret, string.Empty);
    }

    public static OAuthTokenRequest Create(string clientId, string clientSecret, string refreshToken)
    {
        return new OAuthTokenRequest
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            RefreshToken = refreshToken,
        };
    }
}