using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;
using SFA.DAS.TokenService.Domain.Data;
using SFA.DAS.TokenService.Infrastructure.Configuration;

namespace SFA.DAS.TokenService.Infrastructure.Data;

public class KeyVaultSecretRepository(KeyVaultConfiguration configuration, ILogger<KeyVaultSecretRepository> logger) : ISecretRepository
{
    public async Task<string> GetSecretAsync(string name)
    {
        logger.LogInformation("Getting secret {Name} from KeyVault with Uri '{Uri}' using Managed Service Identity", name, configuration.VaultUri);
        
        var vaultUri = new Uri(configuration.VaultUri ?? throw new InvalidOperationException("KeyVault Uri is null"));
        
        var options = new SecretClientOptions
        {
            Retry =
            {
                Delay= TimeSpan.FromSeconds(2),
                MaxDelay = TimeSpan.FromSeconds(16),
                MaxRetries = 5,
                Mode = RetryMode.Exponential
            }
        };

        try
        {
            var client = new SecretClient(vaultUri, new DefaultAzureCredential(), options);
        
            var response = await client.GetSecretAsync(name);

            return response.Value.Value;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Authentication Failed.");
            throw;
        }
    }
}