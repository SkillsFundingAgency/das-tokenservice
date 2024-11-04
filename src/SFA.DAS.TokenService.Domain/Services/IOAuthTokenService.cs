namespace SFA.DAS.TokenService.Domain.Services;

public interface IOAuthTokenService
{
    Task<OAuthAccessToken> GetAccessToken(string privilegedAccessToken);
    Task<OAuthAccessToken> GetAccessToken(string privilegedAccessToken, string refreshToken);
}