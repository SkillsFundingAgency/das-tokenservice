using Azure.Core;
using Azure.Identity;
using Microsoft.Identity.Client;

namespace SFA.DAS.TokenService.Api.Client;

internal class SecureHttpClient(ITokenServiceApiClientConfiguration configuration) : ISecureHttpClient
{
    private readonly DefaultAzureCredential _tokenCredential = new();

    public async Task<string> GetAsync(string url)
    {
        var accessToken = IsClientCredentialConfiguration(configuration.ClientId, configuration.ClientSecret, configuration.Tenant)
            ? await GetClientCredentialAuthenticationResult(configuration.ClientId, configuration.ClientSecret, configuration.IdentifierUri, configuration.Tenant)
            : await GetManagedIdentityAuthenticationResult(configuration.IdentifierUri);

        using var client = new HttpClient();
        
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    private static async Task<string> GetClientCredentialAuthenticationResult(string clientId, string clientSecret, string resource, string tenant)
    {
        var authority = $"https://login.microsoftonline.com/{tenant}";
        var app = ConfidentialClientApplicationBuilder
            .Create(clientId)
            .WithAuthority(authority)
            .Build();

        var userAssertion = new UserAssertion(clientSecret);

        var authResult = await app
            .AcquireTokenOnBehalfOf([$"{resource}/.default"], userAssertion)
            .ExecuteAsync();
            
        return authResult.AccessToken;
    }

    private async Task<string> GetManagedIdentityAuthenticationResult(string resource)
    {
        var accessToken = await _tokenCredential.GetTokenAsync(new TokenRequestContext(scopes: [resource + "/.default"]));
        return accessToken.Token;
    }

    private static bool IsClientCredentialConfiguration(string clientId, string clientSecret, string tenant)
    {
        return !string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret) && !string.IsNullOrEmpty(tenant);
    }
}