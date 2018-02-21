using System;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using NLog;
using SFA.DAS.TokenService.Domain.Data;
using SFA.DAS.TokenService.Infrastructure.Configuration;

namespace SFA.DAS.TokenService.Infrastructure.Data
{
    public class KeyVaultSecretRepository : ISecretRepository
    {
        private readonly KeyVaultConfiguration _configuration;
        private readonly ILogger _logger;

        public KeyVaultSecretRepository(KeyVaultConfiguration configuration, ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GetSecretAsync(string name)
        {
            _logger.Debug($"Getting secret {name} from KeyVault");

            var client = new KeyVaultClient(GetToken, new System.Net.Http.HttpClient());
            var secret = await client.GetSecretAsync(_configuration.VaultUri, name);
            return secret.Value;
        }

        private async Task<string> GetToken(string authority, string resource, string scope)
        {
            _logger.Debug($"Authenticating for KeyVault (authority={authority}, resource={resource}, scope={scope}, ClientId={_configuration.ClientId})");

            var context = new AuthenticationContext(authority);
            var credentials = new ClientCredential(_configuration.ClientId, _configuration.ClientSecret);
            var authenticationResult = await context.AcquireTokenAsync(resource, credentials);

            if (authenticationResult == null)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token");
            }

            _logger.Debug("Successfully authenticated for KeyVault");
            return authenticationResult.AccessToken;
        }
    }
}
