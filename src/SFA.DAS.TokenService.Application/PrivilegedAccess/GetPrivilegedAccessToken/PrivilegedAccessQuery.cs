using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediatR;
using SFA.DAS.TokenService.Domain;

namespace SFA.DAS.TokenService.Application.PrivilegedAccess.GetPrivilegedAccessToken
{
    public class PrivilegedAccessQuery : IAsyncRequest<OAuthAccessToken>
    {
    }
}
