using Microsoft.AspNetCore.Authentication.JwtBearer;
using SFA.DAS.Api.Common.Configuration;

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