using Microsoft.Extensions.Options;
using SFA.DAS.TokenService.Infrastructure.Configuration;

namespace SFA.DAS.TokenService.Api.StartupExtensions;

public static class ConfigurationServiceRegistrations
{
    public static IServiceCollection AddConfigurationOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions();
        
        services.Configure<KeyVaultConfiguration>(configuration);
        services.AddSingleton(cfg => cfg.GetService<IOptions<KeyVaultConfiguration>>()?.Value);

        services.Configure<OAuthTokenServiceConfiguration>(configuration);
        services.AddSingleton(cfg => cfg.GetService<IOptions<OAuthTokenServiceConfiguration>>()?.Value);

        services.AddSingleton(configuration);
        
        return services;
    }
}