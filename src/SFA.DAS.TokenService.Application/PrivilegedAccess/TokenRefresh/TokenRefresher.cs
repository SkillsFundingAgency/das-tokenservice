using Microsoft.Extensions.Logging;
using SFA.DAS.TokenService.Domain;

namespace SFA.DAS.TokenService.Application.PrivilegedAccess.TokenRefresh;

public class TokenRefresher(
    ITokenRefreshAudit refreshAudit,
    TokenRefresherParameters parameters,
    ILogger<TokenRefresher> logger)
    : ITokenRefresher
{
    public async Task StartTokenBackgroundRefreshAsync(
            OAuthAccessToken? token, 
            Func<OAuthAccessToken, Task<OAuthAccessToken>> refreshToken,
            CancellationToken cancellationToken,
            string correlationId)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                token = await WaitAndThenRefreshAsync(token, cancellationToken, refreshToken, correlationId);
            }
        }

        private async Task<OAuthAccessToken> WaitAndThenRefreshAsync(
            OAuthAccessToken token,
            CancellationToken cancellationToken, 
            Func<OAuthAccessToken, Task<OAuthAccessToken>> refreshToken,
            string correlationId)
        {
            var auditItem = refreshAudit.CreateAuditEntry(token);

            await WaitForRefreshTimeAsync(token, cancellationToken, auditItem, correlationId);

            if (!cancellationToken.IsCancellationRequested)
            {
                refreshAudit.RefreshStarted(auditItem);

                var newToken = await TryRefreshUntilCancelledOrSuccess(
                                                        auditItem, 
                                                        token, 
                                                        cancellationToken, 
                                                        refreshToken);

                refreshAudit.RefreshEnded(auditItem);
                return newToken;
            }

            return null;
        }

        private async Task<OAuthAccessToken> TryRefreshUntilCancelledOrSuccess(
            TokenRefreshAuditEntry auditItem,
            OAuthAccessToken token,
            CancellationToken cancellationToken,
            Func<OAuthAccessToken, Task<OAuthAccessToken>> refreshToken)
        {
            OAuthAccessToken newToken;

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

        private Task<OAuthAccessToken> TryRefresh(
            OAuthAccessToken token,
            Func<OAuthAccessToken, Task<OAuthAccessToken>> refreshToken)
        {
            try
            {
                return refreshToken(token);
            }
            catch (Exception)
            {
                // we need to keep trying
                return Task.FromResult<OAuthAccessToken>(null);
            }
        }

        private Task WaitForRefreshTimeAsync(OAuthAccessToken token, CancellationToken cancellationToken, TokenRefreshAuditEntry auditItem, string correlationId)
        {
            var delay = DateTime.UtcNow.GetPercentageTowards(token.ExpiresAt, parameters.TokenRefreshExpirationPercentage);
            auditItem.PlannedRefreshDelay = delay;

            if (delay == TimeSpan.Zero)
            {
                return Task.CompletedTask;
            }

            logger.LogInformation("Waiting for token refresh: {CorreationId} {AuditEntry}", correlationId, auditItem.ToString());
            return Task.Delay(delay, cancellationToken);
        }
    }