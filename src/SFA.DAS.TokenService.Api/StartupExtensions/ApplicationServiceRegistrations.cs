using System.Diagnostics.CodeAnalysis;
using SFA.DAS.TokenService.Application.PrivilegedAccess.TokenRefresh;
using SFA.DAS.TokenService.Domain.Data;
using SFA.DAS.TokenService.Domain.Services;
using SFA.DAS.TokenService.Infrastructure.Data;
using SFA.DAS.TokenService.Infrastructure.ExecutionPolicies;
using SFA.DAS.TokenService.Infrastructure.Http;
using SFA.DAS.TokenService.Infrastructure.OneTimePassword;
using SFA.DAS.TokenService.Infrastructure.Services;

namespace SFA.DAS.TokenService.Api.StartupExtensions;

[ExcludeFromCodeCoverage]
public static class ApplicationServiceRegistrations
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddTransient<ISecretRepository, KeyVaultSecretRepositoryMSIAuth>();
        services.AddTransient<IHmrcAuthTokenBrokerConfig, HmrcAuthTokenBrokerConfig>();
        services.AddSingleton<IHttpClientWrapper, HttpClientWrapper>();
        services.AddSingleton<IOAuthTokenService, OAuthTokenService>();
        services.AddTransient<ExecutionPolicy, HmrcExecutionPolicy>();
        services.AddSingleton<ITokenRefresher, TokenRefresher>();
        services.AddSingleton<IHmrcAuthTokenBroker, HmrcAuthTokenBroker>();
        services.AddSingleton<ITokenRefreshAudit>(new TokenRefreshAudit());
        services.AddSingleton<ITotpService, TotpService>();
        services.AddSingleton(new TokenRefresherParameters { TokenRefreshExpirationPercentage = 80 });

        return services;
    }
}