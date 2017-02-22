using Newtonsoft.Json;

namespace SFA.DAS.TokenService.Infrastructure.Services
{
    public class OAuthTokenRefreshRequest : OAuthTokenRequest
    {
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
    }
}