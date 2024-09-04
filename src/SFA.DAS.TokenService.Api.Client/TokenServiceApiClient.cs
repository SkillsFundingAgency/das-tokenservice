using Newtonsoft.Json;
using SFA.DAS.TokenService.Api.Types;

namespace SFA.DAS.TokenService.Api.Client;

public class TokenServiceApiClient(ITokenServiceApiClientConfiguration configuration, ISecureHttpClient httpClient) : ITokenServiceApiClient
{
    public TokenServiceApiClient(ITokenServiceApiClientConfiguration configuration) : this(configuration, new SecureHttpClient(configuration))
    {
    }

    public async Task<PrivilegedAccessToken?> GetPrivilegedAccessTokenAsync()
    {
        var absoluteUri = new Uri(new Uri(configuration.ApiBaseUrl!), "api/PrivilegedAccess");
        var json = await httpClient.GetAsync(absoluteUri.ToString());
        
        return JsonConvert.DeserializeObject<PrivilegedAccessToken>(json);
    }
}