using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using SFA.DAS.TokenService.Domain.Data;
using SFA.DAS.TokenService.Infrastructure.Configuration;
using SFA.DAS.TokenService.Infrastructure.Data;

namespace SFA.DAS.TokenService.Api.StartupExtensions;

[ExcludeFromCodeCoverage]
public static class ConfigurationServiceRegistrations
{
    public static IServiceCollection AddConfigurationOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions();

        services.AddSingleton(new KeyVaultConfiguration
        {
            VaultUri = configuration["KeyVaultUri"]
        });

        services.AddSingleton(new OAuthTokenServiceConfiguration
        {
            Url = configuration["HmrcTokenUri"],
            ClientId = configuration["HmrcTokenClientId"],
            ClientSecret = configuration["HmrcTokenSecret"]
        });

        services.AddSingleton(configuration);

        return services;
    }
}