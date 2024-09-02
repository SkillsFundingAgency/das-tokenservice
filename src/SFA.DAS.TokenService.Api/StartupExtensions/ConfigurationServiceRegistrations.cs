using SFA.DAS.TokenService.Infrastructure.Configuration;

namespace SFA.DAS.TokenService.Api.StartupExtensions;

public static class ConfigurationServiceRegistrations
{
    public static IServiceCollection AddConfigurationOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions();
        
        services.Configure<KeyVaultConfiguration>(configuration);
        services.Configure<OAuthTokenServiceConfiguration>(configuration);
        
        return services;
    }
}