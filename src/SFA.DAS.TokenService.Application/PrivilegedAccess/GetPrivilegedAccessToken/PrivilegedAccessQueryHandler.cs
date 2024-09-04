using MediatR;
using SFA.DAS.TokenService.Application.PrivilegedAccess.TokenRefresh;
using SFA.DAS.TokenService.Domain;

namespace SFA.DAS.TokenService.Application.PrivilegedAccess.GetPrivilegedAccessToken;

public class PrivilegedAccessQueryHandler(IHmrcAuthTokenBroker hmrcAuthTokenBroker) : IRequestHandler<PrivilegedAccessQuery, OAuthAccessToken>
{
    public async Task<OAuthAccessToken?> Handle(PrivilegedAccessQuery request, CancellationToken cancellationToken)
    {
        return await hmrcAuthTokenBroker.GetTokenAsync();
    }
}