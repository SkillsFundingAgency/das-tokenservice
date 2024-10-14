using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.TokenService.Application.PrivilegedAccess.TokenRefresh;
using SFA.DAS.TokenService.Domain;

namespace SFA.DAS.TokenService.Application.UnitTests.PrivilegedAccess.TokenRefresh.TokenRefresherTests;

public class WhenBackgroundTokenRefreshRequested
{
    [TestCase(2000, 100, 80)]
    [TestCase(2000, 500, 80)]
    [TestCase(2000, 1000, 80)]
    public async Task WeShouldNotRunRefreshInParallel(
        int runDurationMsecs,
        int tokenLifetimeMsecs,
        int refreshExpirationPercentage)
    {
        var auditItems = await RunRefresherAndReturnCompletedRefreshInfo(runDurationMsecs, tokenLifetimeMsecs, refreshExpirationPercentage);

        for (var index = 1; index < auditItems.Count; index++)
        {
            auditItems[index].ActualRefreshStart!.Value.Should().BeAfter(auditItems[index - 1].ActualRefreshEnd!.Value);
        }
    }

    [TestCase(2000, 500, 80)]
    [TestCase(2000, 750, 60)]
    [TestCase(2000, 1000, 80)]
    public async Task WeShouldAlwaysHaveAValidToken(
        int runDurationMsecs,
        int tokenLifetimeMsecs,
        int refreshExpirationPercentage)
    {
        var auditItems = await RunRefresherAndReturnCompletedRefreshInfo(runDurationMsecs, tokenLifetimeMsecs, refreshExpirationPercentage);

        auditItems.All(aie => aie.ActualRefreshStart <= aie.ExpirationTime).Should().BeTrue();
    }

    private static async Task<List<TokenRefreshAuditEntry>> RunRefresherAndReturnCompletedRefreshInfo(
        int runDurationMsecs,
        int tokenLifetimeMsecs,
        int refreshExpirationPercentage,
        int minimumNumberOfRefreshes = 1)
    {
        // Arrange
        var audit = new TokenRefreshAudit(true);

        var refresher = new TokenRefresher(audit, new TokenRefresherParameters { TokenRefreshExpirationPercentage = refreshExpirationPercentage });

        var cancellationSource = new CancellationTokenSource(runDurationMsecs);
        var token = AccessTokenBuilder.Create().WithValidState().ExpiresInMSecs(tokenLifetimeMsecs);

        // Act
        _ = refresher.StartTokenBackgroundRefreshAsync(token,
            t =>
            {
                var newToken = AccessTokenBuilder.Create().WithValidState().ExpiresInMSecs(tokenLifetimeMsecs);
                return Task.FromResult(newToken)!;
            }, cancellationSource.Token);

        try
        {
            await Task.Delay(runDurationMsecs, cancellationSource.Token);
        }
        catch (TaskCanceledException)
        {
            // We're only expecting cancellation exceptions
        }
        catch (AggregateException aggregateException)
        {
            if (aggregateException.Flatten().InnerExceptions.Any(innerException => innerException is not TaskCanceledException))
            {
                // We're only expecting cancellation exceptions
                throw;
            }
        }

        cancellationSource.Dispose();
        var result = audit.AuditItems
            .Where(refreshAuditEntry => refreshAuditEntry.ActualRefreshStart.HasValue).OrderBy(aie => aie.ActualRefreshStart)
            .ToList();

        var estimatedMaximumNumberOfRefreshes = runDurationMsecs / tokenLifetimeMsecs * refreshExpirationPercentage;

        result.Count.Should().BeGreaterThan(minimumNumberOfRefreshes, "There was an unexpectedly low number of refreshes");
        result.Count.Should().BeLessThan(estimatedMaximumNumberOfRefreshes, "There was an unexpectedly high number of refreshes");

        return result;
    }
}

public static class AccessTokenBuilder
{
    public static OAuthAccessToken Create() => new();

    public static OAuthAccessToken WithValidState(this OAuthAccessToken token)
    {
        token.WithRandomTokens();
        token.ExpiresAt = DateTime.UtcNow.AddHours(1);
        return token;
    }

    private static OAuthAccessToken WithRandomTokens(this OAuthAccessToken token)
    {
        token.AccessToken = Guid.NewGuid().ToString();
        token.RefreshToken = Guid.NewGuid().ToString();

        return token;
    }

    public static OAuthAccessToken ExpiresInMSecs(this OAuthAccessToken? token, int msecs)
    {
        token!.ExpiresAt = DateTime.UtcNow.AddMilliseconds(msecs);
        return token;
    }
}