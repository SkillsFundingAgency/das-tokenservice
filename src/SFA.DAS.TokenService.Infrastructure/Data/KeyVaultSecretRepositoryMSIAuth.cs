using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;
using SFA.DAS.TokenService.Domain.Data;
using SFA.DAS.TokenService.Infrastructure.Configuration;

namespace SFA.DAS.TokenService.Infrastructure.Data;

public class KeyVaultSecretRepositoryMSIAuth(KeyVaultConfiguration configuration, ILogger<KeyVaultSecretRepositoryMSIAuth> logger) : ISecretRepository
{
    public async Task<string> GetSecretAsync(string name)
    {
        logger.LogDebug("Getting secret {Name} from KeyVault using Managed Service Identity", name);
       
        var vaultUri = new Uri(configuration.VaultUri ?? throw new InvalidOperationException("KeyVault Uri is null"));
        var client = new SecretClient(vaultUri, new DefaultAzureCredential());
        var response = await client.GetSecretAsync(configuration.VaultUri, name);

        return response.Value.Value;
    }
}