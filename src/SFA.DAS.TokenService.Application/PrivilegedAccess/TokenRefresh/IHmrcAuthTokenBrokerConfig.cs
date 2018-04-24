using System;

namespace SFA.DAS.TokenService.Application.PrivilegedAccess.TokenRefresh
{
    public interface IHmrcAuthTokenBrokerConfig
    {
        TimeSpan RetryDelay { get; }
    }
}