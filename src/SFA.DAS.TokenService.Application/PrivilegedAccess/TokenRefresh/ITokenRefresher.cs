using SFA.DAS.TokenService.Domain;

namespace SFA.DAS.TokenService.Application.PrivilegedAccess.TokenRefresh;

/// <summary>
///     Represents a service that will keep refreshing the supplied token until the 
///     cancellation token is set cancelled.
/// </summary>
/// <remarks>
///     The concrete implementation (<see cref="TokenRefresher"/> will start refreshing
///     the token when it is 80% towards the end of its life (see <see cref="TokenRefresherParameters.TokenRefreshExpirationPercentage"/>).
///     Once successfully refreshed the existing token will be swapped out.
/// </remarks>
public interface ITokenRefresher
{
    /// <summary>
    ///     Starts a background task that will run indefinitely refreshing the supplied token.
    /// </summary>
    /// <param name="token">The original token</param>
    /// <param name="refreshToken">
    ///     A delegate that will be called to refresh the token when the existing token is getting towards
    ///     the end of its life.
    /// </param>
    /// <param name="cancellationToken">
    ///     A cancellation token provided by the caller. This should be cancelled when the caller is closing 
    ///     down.
    /// </param>
    /// <param name="correlationId">
    ///     A correlationId provided by the caller.
    ///     down.
    /// </param>
    /// <returns>
    ///     The background task. This should probably not be awaited as it will run for a long time.
    /// </returns>
    Task StartTokenBackgroundRefreshAsync(OAuthAccessToken? token,
        Func<OAuthAccessToken, Task<OAuthAccessToken?>> refreshToken,
        CancellationToken cancellationToken,
        string correlationId);
}