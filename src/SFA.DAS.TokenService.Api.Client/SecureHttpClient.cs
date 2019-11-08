using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace SFA.DAS.TokenService.Api.Client
{
    internal class SecureHttpClient : ISecureHttpClient
    {
        private readonly ITokenServiceApiClientConfiguration _configuration;

        public SecureHttpClient(ITokenServiceApiClientConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<string> GetAsync(string url)
        {
            var authenticationResult = await GetAuthenticationResult(_configuration.ClientId, _configuration.ClientSecret, _configuration.IdentifierUri, _configuration.Tenant);
            using (var handler = new WebRequestHandler())
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);

                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
        }

        private async Task<AuthenticationResult> GetAuthenticationResult(string clientId, string appKey, string resourceId, string tenant)
        {
            var authority = $"https://login.microsoftonline.com/{tenant}";
            var clientCredential = new ClientCredential(clientId, appKey);
            var context = new AuthenticationContext(authority, true);
            var result = await context.AcquireTokenAsync(resourceId, clientCredential);
            return result;
        }
    }
}