using System;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using NLog;
using SFA.DAS.TokenService.Domain.Data;
using SFA.DAS.TokenService.Infrastructure.Configuration;

namespace SFA.DAS.TokenService.Infrastructure.Data
{
    public class KeyVaultSecretRepositoryMSIAuth : ISecretRepository
    {
        private readonly KeyVaultConfiguration _configuration;
        private readonly ILogger _logger;

        public KeyVaultSecretRepositoryMSIAuth(KeyVaultConfiguration configuration, ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GetSecretAsync(string name)
        {
            _logger.Debug($"Getting secret {name} from KeyVault using Managed Service Identity");
            AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
            KeyVaultClient keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

            var secret = await keyVaultClient.GetSecretAsync(_configuration.VaultUri, name);
            return secret.Value;
        }
    }
}
