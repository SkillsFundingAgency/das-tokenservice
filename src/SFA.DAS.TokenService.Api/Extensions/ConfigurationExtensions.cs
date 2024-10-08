namespace SFA.DAS.TokenService.Api.Extensions;

public static class ConfigurationExtensions
{
    private static bool IsDev(this IConfiguration configuration)
    {
        var isDev = configuration["EnvironmentName"]?.StartsWith("DEV", StringComparison.CurrentCultureIgnoreCase) ?? false;
        var isDevelopment = configuration["EnvironmentName"]?.StartsWith("Development", StringComparison.CurrentCultureIgnoreCase) ?? false;

        return isDev || isDevelopment;
    }

    private static bool IsLocal(this IConfiguration configuration)
    {
        return configuration["EnvironmentName"]?.StartsWith("LOCAL", StringComparison.CurrentCultureIgnoreCase) ?? false;
    }

    public static bool IsDevOrLocal(this IConfiguration configuration)
    {
        return IsDev(configuration) || IsLocal(configuration);
    }
}