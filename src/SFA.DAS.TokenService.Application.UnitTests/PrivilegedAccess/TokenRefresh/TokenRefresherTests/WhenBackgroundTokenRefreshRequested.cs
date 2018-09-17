using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NLog;
using NUnit.Framework;
using SFA.DAS.TokenService.Application.PrivilegedAccess.TokenRefresh;
using SFA.DAS.TokenService.Domain;

namespace SFA.DAS.TokenService.Application.UnitTests.PrivilegedAccess.TokenRefresh.TokenRefresherTests
{
    [TestFixture]
    public class WhenBackgroundTokenRefreshRequested
    {
        [TestCase(2000,  100, 80)]
        [TestCase(2000,  500, 80)]
        [TestCase(2000, 1000, 80)]
        public async Task WeShouldNotRunRefreshInParallel(
            int runDurationMsecs,
            int tokenLifetimeMsecs,
            int refreshExpirationPercentage)
        {
            var auditItems = await RunRefresherAndReturnCompletedRefreshInfo(runDurationMsecs, tokenLifetimeMsecs, refreshExpirationPercentage);

            for (int i = 1; i < auditItems.Count; i++)
            {
                Assert.Greater(auditItems[i].ActualRefreshStart, auditItems[i-1].ActualRefreshEnd, "One refresh started before the previous finished");
            }
        }

        [TestCase(2000,  500, 80)]
        [TestCase(2000,  750, 60)]
        [TestCase(2000, 1000, 80)]
        public async Task WeShouldAlwaysHaveAValidToken(
            int runDurationMsecs,
            int tokenLifetimeMsecs,
            int refreshExpirationPercentage)
        {
            var auditItems = await RunRefresherAndReturnCompletedRefreshInfo(runDurationMsecs, tokenLifetimeMsecs, refreshExpirationPercentage);

            Assert.IsTrue(auditItems.All(aie => aie.ActualRefreshStart <= aie.ExpirationTime));
        }

        private async Task<List<TokenRefreshAuditEntry>> RunRefresherAndReturnCompletedRefreshInfo(
            int runDurationMsecs,
            int tokenLifetimeMsecs,
            int refreshExpirationPercentage,
            int minimumNumberOfRefreshes = 1)
        {
            // Arrange
            var audit = new TokenRefreshAudit(true);

            var refresher =
                new TokenRefresher(audit, new TokenRefresherParameters { TokenRefreshExpirationPercentage = refreshExpirationPercentage });

            var cancellationSource = new CancellationTokenSource(runDurationMsecs);
            var token = AccessTokenBuilder.Create().WithValidState().ExpiresInMSecs(tokenLifetimeMsecs);

            // Act
            var refreshTask = refresher.StartTokenBackgroundRefreshAsync(token, cancellationSource.Token,
                t =>
                {
                    
                    var newToken = AccessTokenBuilder.Create().WithValidState().ExpiresInMSecs(tokenLifetimeMsecs);
                    return Task.FromResult(newToken);
                });

            try
            {
                await Task.Delay(runDurationMsecs);
                refreshTask.Wait(20); // allow a little bit extra to let the task end cleanly
            }
            catch (AggregateException e)
            {
                if (e.Flatten().InnerExceptions.Any(innerException => !(innerException is TaskCanceledException)))
                {
                    // We're only expecting cancellation exceptions
                    throw;
                }
            }

            cancellationSource.Dispose();
            var result = audit.AuditItems
                .Where(aie => aie.ActualRefreshStart.HasValue).OrderBy(aie => aie.ActualRefreshStart)
                .ToList();

            var estimatedMaximumNumberOfRefreshes = runDurationMsecs / tokenLifetimeMsecs * refreshExpirationPercentage;

            Assert.Greater(result.Count, minimumNumberOfRefreshes, "There was an unexpectedly low number of refreshes");
            Assert.Less(result.Count, estimatedMaximumNumberOfRefreshes, "There was an unexpectedly high number of refreshes");
            return result;
        }
    }


    public static class AccessTokenBuilder
    {
        public static OAuthAccessToken Create()
        {
            return new OAuthAccessToken();
        }

        public static OAuthAccessToken WithValidState(this OAuthAccessToken token)
        {
            token.WithRandomTokens();
            token.ExpiresAt = DateTime.UtcNow.AddHours(1);
            return token;
        }

        public static OAuthAccessToken WithRandomTokens(this OAuthAccessToken token)
        {
            token.AccessToken = Guid.NewGuid().ToString();
            token.RefreshToken = Guid.NewGuid().ToString();
            return token;
        }

        public static OAuthAccessToken ExpiresInMSecs(this OAuthAccessToken token, int msecs)
        {
            token.ExpiresAt = DateTime.UtcNow.AddMilliseconds(msecs);
            return token;
        }
    }
}
