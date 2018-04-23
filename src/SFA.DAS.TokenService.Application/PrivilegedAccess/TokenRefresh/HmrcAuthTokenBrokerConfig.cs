using System;

namespace SFA.DAS.TokenService.Application.PrivilegedAccess.TokenRefresh
{
    public class HmrcAuthTokenBrokerConfig : IHmrcAuthTokenBrokerConfig
    {
        public HmrcAuthTokenBrokerConfig()
        {
            RetryDelay = TimeSpan.FromSeconds(30);    
        }

        public TimeSpan RetryDelay { get; set; }
    }
}