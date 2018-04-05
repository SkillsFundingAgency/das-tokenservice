using System.Threading.Tasks;
using MediatR;
using SFA.DAS.TokenService.Application.PrivilegedAccess.TokenRefresh;
using SFA.DAS.TokenService.Domain;

namespace SFA.DAS.TokenService.Application.PrivilegedAccess.GetPrivilegedAccessToken
{
    public class PrivilegedAccessQueryHandler : IAsyncRequestHandler<PrivilegedAccessQuery, OAuthAccessToken>
    {
        private readonly IHmrcAuthTokenBroker _hmrcAuthTokenBroker;

        public PrivilegedAccessQueryHandler(IHmrcAuthTokenBroker hmrcAuthTokenBroker)
        {
            _hmrcAuthTokenBroker = hmrcAuthTokenBroker;
        }

        public Task<OAuthAccessToken> Handle(PrivilegedAccessQuery message)
        {
            return _hmrcAuthTokenBroker.GetTokenAsync();
        }
    }
}