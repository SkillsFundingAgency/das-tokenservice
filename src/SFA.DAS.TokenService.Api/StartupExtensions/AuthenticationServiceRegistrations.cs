using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using SFA.DAS.Api.Common.AppStart;
using SFA.DAS.Api.Common.Configuration;
using SFA.DAS.Api.Common.Infrastructure.Configuration;
using SFA.DAS.TokenService.Api.Authentication;
using SFA.DAS.TokenService.Api.Extensions;

namespace SFA.DAS.TokenService.Api.StartupExtensions;

public static class AuthenticationServiceRegistrations
{
    public static void AddActiveDirectoryAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var activeDirectorySettings = configuration.GetSection(ConfigurationKeys.AzureActiveDirectoryApiConfiguration).Get<AzureActiveDirectoryConfiguration>();

        services.AddAuthorizationBuilder()
            .AddPolicy("default", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole("Default");
            });

        services.AddAuthentication(auth =>
        {
            auth.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(auth =>
        {
            auth.Authority = $"https://login.microsoftonline.com/{activeDirectorySettings?.Tenant}";
            auth.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidAudiences = activeDirectorySettings?.Identifier.Split(','),
            };
        });
    }
}