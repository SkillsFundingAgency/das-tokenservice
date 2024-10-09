using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using SFA.DAS.Api.Common.Configuration;
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
        
        services.AddConfigurationFor<AzureActiveDirectoryConfiguration>(configuration, ConfigurationKeys.AzureActiveDirectoryApiConfiguration);

        services.AddSingleton(new KeyVaultConfiguration
        {
            VaultUri = configuration.GetValue<string>("KeyVaultUri")
        });

        services.AddSingleton(new OAuthTokenServiceConfiguration
        {
            Url = configuration.GetValue<string>("HmrcTokenUri"),
            ClientId = configuration.GetValue<string>("HmrcTokenClientId"),
            ClientSecret = configuration.GetValue<string>("HmrcTokenSecret")
        });

        services.AddSingleton(configuration);

        return services;
    }
    
    private static void AddConfigurationFor<T>(this IServiceCollection services, IConfiguration configuration,
        string key) where T : class => services.AddSingleton(GetConfigurationFor<T>(configuration, key));

    private static T? GetConfigurationFor<T>(IConfiguration configuration, string name)
    {
        var section = configuration.GetSection(name);
        return section.Get<T>();
    }
}