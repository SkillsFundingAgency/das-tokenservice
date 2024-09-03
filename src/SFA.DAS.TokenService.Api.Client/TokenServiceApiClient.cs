using Newtonsoft.Json;
using SFA.DAS.TokenService.Api.Types;

namespace SFA.DAS.TokenService.Api.Client;

public class TokenServiceApiClient : ITokenServiceApiClient
{
    private readonly ITokenServiceApiClientConfiguration _configuration;
    private readonly ISecureHttpClient _httpClient;

    public TokenServiceApiClient(ITokenServiceApiClientConfiguration configuration) : this(configuration, new SecureHttpClient(configuration))
    {
    }
    
    public TokenServiceApiClient(ITokenServiceApiClientConfiguration configuration, ISecureHttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
    }

    public async Task<PrivilegedAccessToken?> GetPrivilegedAccessTokenAsync()
    {
        var absoluteUri = new Uri(new Uri(_configuration.ApiBaseUrl), "api/PrivilegedAccess");
        var json = await _httpClient.GetAsync(absoluteUri.ToString());
        
        return JsonConvert.DeserializeObject<PrivilegedAccessToken>(json);
    }
}