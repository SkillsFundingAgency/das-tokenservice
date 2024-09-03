using SFA.DAS.TokenService.Api.Types;

namespace SFA.DAS.TokenService.Api.Client;

public interface ITokenServiceApiClient
{
    Task<PrivilegedAccessToken?> GetPrivilegedAccessTokenAsync();
}