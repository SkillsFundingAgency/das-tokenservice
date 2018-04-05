using SFA.DAS.TokenService.Application.PrivilegedAccess.TokenRefresh;
using SFA.DAS.TokenService.Infrastructure.Configuration;
using StructureMap;
using StructureMap.Graph;

namespace SFA.DAS.TokenService.Api.DependencyResolution
{

    public class TokenRefreshRegistry : Registry
    {
        public TokenRefreshRegistry()
        {
            For<ITokenRefresher>().Use<TokenRefresher>().Singleton();
            For<IHmrcAuthTokenBroker>().Use<HmrcAuthTokenBroker>().Singleton();
            For<TokenRefresherParameters>().Use(new TokenRefresherParameters() {TokenRefreshExpirationPercentage = 80})
                .Singleton();
            For<ITokenRefreshAudit>().Use(new TokenRefreshAudit(false)).Singleton();
        }
    }
}