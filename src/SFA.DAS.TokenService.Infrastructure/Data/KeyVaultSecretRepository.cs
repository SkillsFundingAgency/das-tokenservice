using System;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using SFA.DAS.TokenService.Domain.Data;
using SFA.DAS.TokenService.Infrastructure.Configuration;

namespace SFA.DAS.TokenService.Infrastructure.Data
{
    public class KeyVaultSecretRepository : ISecretRepository
    {
        private readonly KeyVaultConfiguration _configuration;

        public KeyVaultSecretRepository(KeyVaultConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> GetSecretAsync(string name)
        {
            var client = new KeyVaultClient(GetToken, new System.Net.Http.HttpClient());
            var secret = await client.GetSecretAsync(_configuration.VaultUri, name);
            return secret.Value;
        }

        private async Task<string> GetToken(string authority, string resource, string scope)
        {
            var context = new AuthenticationContext(authority);
            var credentials = new ClientCredential(_configuration.ClientId, _configuration.ClientSecret);
            var authenticationResult = await context.AcquireTokenAsync(resource, credentials);

            if (authenticationResult == null)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token");
            }

            return authenticationResult.AccessToken;
        }
    }
}
