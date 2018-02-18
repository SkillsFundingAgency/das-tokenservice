using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.TokenService.Domain;

namespace SFA.DAS.TokenService.Application.PrivilegedAccess.TokenRefresh
{
    public class TokenRefresher : ITokenRefresher
    {
        private readonly TokenRefresherParameters _parameters;
        private readonly ITokenRefreshAudit _refreshAudit;

        public TokenRefresher(
            ITokenRefreshAudit refreshAudit,
            TokenRefresherParameters parameters)
        {
            _parameters = parameters;
            _refreshAudit = refreshAudit;
        }

        public Task StartTokenBackgroundRefreshAsync(
            OAuthAccessToken token, 
            CancellationToken cancellationToken, 
            Func<OAuthAccessToken, Task<OAuthAccessToken>> refreshToken)
        {
            return Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    token = await WaitAndThenRefreshAsync(token, cancellationToken, refreshToken);
                }
            }, cancellationToken);
        }

        private async Task<OAuthAccessToken> WaitAndThenRefreshAsync(
            OAuthAccessToken token,
            CancellationToken cancellationToken, 
            Func<OAuthAccessToken, Task<OAuthAccessToken>> refreshToken)
        {
            var auditItem = _refreshAudit.CreateAuditEntry(token);

            await WaitForRefreshTimeAsync(token, cancellationToken, auditItem);

            if (!cancellationToken.IsCancellationRequested)
            {
                _refreshAudit.StartRefresh(auditItem);

                OAuthAccessToken newToken = await TryRefreshUntilCancelledOrSuccess(
                                                        auditItem, 
                                                        token, 
                                                        cancellationToken, 
                                                        refreshToken);

                _refreshAudit.EndRefresh(auditItem);
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
                newToken = await TryRefresh(token, cancellationToken, refreshToken);
                if (newToken == null)
                {
                    await Task.Delay(_parameters.RetryInterval, cancellationToken);
                }
            } while (newToken == null && !cancellationToken.IsCancellationRequested);

            return newToken;
        }

        private Task<OAuthAccessToken> TryRefresh(
            OAuthAccessToken token,
            CancellationToken cancellationToken,
            Func<OAuthAccessToken, Task<OAuthAccessToken>> refreshToken)
        {
            try
            {
                return refreshToken(token);
            }
            catch (Exception e)
            {
                // we need to keep trying
                return Task.FromResult<OAuthAccessToken>(null);
            }
        }

        //TODO: not using 4.6.2 so Task.Completed not available
        private readonly Task _completedTask = Task.Run(() => {});

        private Task WaitForRefreshTimeAsync(OAuthAccessToken token, CancellationToken cancellationToken, TokenRefreshAuditEntry auditItem)
        {
            var delay = DateTime.UtcNow.GetPercentageTowards(token.ExpiresAt, _parameters.TokenRefreshExpirationPercentage);
            auditItem.PlannedRefreshDelay = delay;

            if (delay == TimeSpan.Zero)
            {
                return _completedTask;
            }

            return Task.Delay(delay, cancellationToken);
        }
    }
}