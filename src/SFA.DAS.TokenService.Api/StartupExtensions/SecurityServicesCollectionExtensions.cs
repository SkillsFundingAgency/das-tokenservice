using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace SFA.DAS.TokenService.Api.StartupExtensions;

[ExcludeFromCodeCoverage]
public static class SecurityServicesCollectionExtensions
{
    public static void AddActiveDirectoryAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy("default", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole("PrivilegedAccess");
            });
        
        services.AddAuthentication(auth =>
        {
            auth.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

        }).AddJwtBearer(auth =>
        {
            auth.Authority = $"https://login.microsoftonline.com/{configuration["Tenant"]}";
            auth.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidAudiences = configuration["idaAudience"].Split(','),
            };
        });
    }
}