using SFA.DAS.TokenService.Domain;

namespace SFA.DAS.TokenService.Application.PrivilegedAccess.TokenRefresh;

public class TokenRefresher(
    ITokenRefreshAudit refreshAudit,
    TokenRefresherParameters parameters)
    : ITokenRefresher
{
    public Task StartTokenBackgroundRefreshAsync(OAuthAccessToken? token,
        Func<OAuthAccessToken, Task<OAuthAccessToken?>> refreshToken,
        CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                token = await WaitAndThenRefreshAsync(token, cancellationToken, refreshToken);
            }
        }, cancellationToken);
    }

    private async Task<OAuthAccessToken?> WaitAndThenRefreshAsync(
        OAuthAccessToken? token,
        CancellationToken cancellationToken,
        Func<OAuthAccessToken, Task<OAuthAccessToken?>> refreshToken)
    {
        var auditItem = refreshAudit.CreateAuditEntry(token);

        await WaitForRefreshTimeAsync(token, auditItem, cancellationToken);

        if (cancellationToken.IsCancellationRequested)
        {
            return null;
        }

        refreshAudit.RefreshStarted(auditItem);

        var newToken = await TryRefreshUntilCancelledOrSuccess(
            auditItem,
            token,
            cancellationToken,
            refreshToken);

        refreshAudit.RefreshEnded(auditItem);

        return newToken;
    }

    private async Task<OAuthAccessToken?> TryRefreshUntilCancelledOrSuccess(
        TokenRefreshAuditEntry auditItem,
        OAuthAccessToken? token,
        CancellationToken cancellationToken,
        Func<OAuthAccessToken, Task<OAuthAccessToken?>> refreshToken)
    {
        OAuthAccessToken? newToken;

        do
        {
            auditItem.RefreshAttemps++;
            newToken = await TryRefresh(token, refreshToken);
            if (newToken == null)
            {
                await Task.Delay(parameters.RetryInterval, cancellationToken);
            }
        } while (newToken == null && !cancellationToken.IsCancellationRequested);

        return newToken;
    }

    private static Task<OAuthAccessToken?> TryRefresh(
        OAuthAccessToken? token,
        Func<OAuthAccessToken, Task<OAuthAccessToken?>> refreshToken)
    {
        try
        {
            return refreshToken(token!);
        }
        catch (Exception)
        {
            // we need to keep trying
            return Task.FromResult<OAuthAccessToken?>(null);
        }
    }

    private readonly Task _completedTask = Task.CompletedTask;

    private Task WaitForRefreshTimeAsync(OAuthAccessToken? token, TokenRefreshAuditEntry auditItem, CancellationToken cancellationToken)
    {
        var delay = DateTime.UtcNow.GetPercentageTowards(token!.ExpiresAt, parameters.TokenRefreshExpirationPercentage);
        auditItem.PlannedRefreshDelay = delay;

        return delay == TimeSpan.Zero ? _completedTask : Task.Delay(delay, cancellationToken);
    }
}