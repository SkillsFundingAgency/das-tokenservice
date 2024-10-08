using Microsoft.AspNetCore.Authentication;
using SFA.DAS.Api.Common.AppStart;
using SFA.DAS.Api.Common.Configuration;
using SFA.DAS.Api.Common.Infrastructure.Configuration;
using SFA.DAS.TokenService.Api.Authentication;
using SFA.DAS.TokenService.Api.Extensions;

namespace SFA.DAS.TokenService.Api.StartupExtensions;

public static class AuthenticationServiceRegistrations
{
    private const string BasicAuthScheme = "BasicAuthentication";
    public static IServiceCollection AddApiAuthentication(this IServiceCollection services, IConfiguration config)
    {
        if (config.IsDevOrLocal())
        {
            services
                .AddAuthentication(BasicAuthScheme)
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(BasicAuthScheme, null);
        }
        else
        {
            var azureAdConfiguration = config
                .GetSection(ConfigurationKeys.AzureActiveDirectoryApiConfiguration)
                .Get<AzureActiveDirectoryConfiguration>();

            var policies = new Dictionary<string, string> { { PolicyNames.Default, RoleNames.Default } };
            
            services.AddAuthentication(azureAdConfiguration, policies);
            services.AddSingleton<IClaimsTransformation, AzureAdScopeClaimTransformation>();
        }

        return services;
    }
}