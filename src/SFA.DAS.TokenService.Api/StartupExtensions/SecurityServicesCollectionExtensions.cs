using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using SFA.DAS.Api.Common.AppStart;
using SFA.DAS.Api.Common.Configuration;
using SFA.DAS.Api.Common.Infrastructure.Configuration;
using SFA.DAS.TokenService.Api.Authentication;
using SFA.DAS.TokenService.Api.Extensions;

namespace SFA.DAS.TokenService.Api.StartupExtensions;

[ExcludeFromCodeCoverage]
public static class SecurityServicesCollectionExtensions
{
    private const string BasicAuthScheme = "BasicAuthentication";
    
    public static void AddDasAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(PolicyNames.Default, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole("PrivilegedAccess");
            });
    }

    public static IServiceCollection AddDasAuthentication(this IServiceCollection services, IConfiguration config)
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