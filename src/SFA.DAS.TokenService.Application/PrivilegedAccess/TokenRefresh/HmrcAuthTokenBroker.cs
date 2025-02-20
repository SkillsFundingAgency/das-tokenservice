using Microsoft.Extensions.Logging;
using SFA.DAS.TokenService.Domain;
using SFA.DAS.TokenService.Domain.Data;
using SFA.DAS.TokenService.Domain.Services;
using SFA.DAS.TokenService.Infrastructure.ExecutionPolicies;

namespace SFA.DAS.TokenService.Application.PrivilegedAccess.TokenRefresh;

public sealed class HmrcAuthTokenBroker(
    ExecutionPolicy executionPolicy,
    ILogger<HmrcAuthTokenBroker> logger,
    IOAuthTokenService tokenService,
    ISecretRepository secretRepository,
    ITotpService totpService,
    ITokenRefresher tokenRefresher,
    IHmrcAuthTokenBrokerConfig hmrcAuthTokenBrokerConfig)
    : IHmrcAuthTokenBroker, IAsyncDisposable
{
    private const string PrivilegedAccessSecretName = "PrivilegedAccessSecret";

    private OAuthAccessToken _cachedAccessToken = null!;
    private CancellationTokenSource? _cancellationTokenSource;

    public async Task<OAuthAccessToken> GetTokenAsync()
    {
        if (_cachedAccessToken != null && _cachedAccessToken.ExpiresAt > DateTime.UtcNow) return _cachedAccessToken;

        _cachedAccessToken = await GetTokenFromServiceAsync();
        StartTokenBackgroundRefresh(_cachedAccessToken);
        return _cachedAccessToken;
    }

    private async Task StartTokenBackgroundRefresh(OAuthAccessToken token)
    {
        await DisposeCancellationTokenAsync();
        _cancellationTokenSource = new CancellationTokenSource();
        _ = tokenRefresher.StartTokenBackgroundRefreshAsync(token, RefreshTokenAsync, _cancellationTokenSource.Token);
    }

    private async Task<OAuthAccessToken> RefreshTokenAsync(OAuthAccessToken existingToken)
    {
        var newToken = await GetTokenFromServiceUsingRefreshTokenAsync(existingToken) ??
                       await GetTokenFromServiceAsync();
        if (newToken != null)
        {
            _cachedAccessToken = newToken;
        }

        return _cachedAccessToken;
    }

    private async Task<OAuthAccessToken?> GetTokenFromServiceUsingRefreshTokenAsync(OAuthAccessToken token)
    {
        try
        {
            logger.LogDebug("Refreshing token (expired {ExpiresAt})", token.ExpiresAt);
            var privilegedAccessToken = await GetPrivilegedAccessTokenAsync();
            var newToken = await executionPolicy.ExecuteAsync(() =>
                tokenService.GetAccessToken(privilegedAccessToken, token.RefreshToken));
            logger.LogDebug("Refresh token successful (new expiry {NewExpiry})", newToken?.ExpiresAt);
            return newToken;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error trying to refresh access token: {Message}", ex.Message);
            return null;
        }
    }

    private async Task<OAuthAccessToken> GetTokenFromServiceAsync()
    {
        var attempts = 0;
        OAuthAccessToken? tempToken = null;

        while (tempToken == null)
        {
            logger.LogDebug("Attempting to get a new token: attempt {Attempts}", ++attempts);
            var privilegedAccessToken = await GetPrivilegedAccessTokenAsync();
            tempToken = await executionPolicy.ExecuteAsync(() =>
                tokenService.GetAccessToken(privilegedAccessToken));

            if (tempToken == null)
            {
                logger.LogWarning("Attempt to get a token failed - retrying in {RetryDelay} ms",
                    hmrcAuthTokenBrokerConfig.RetryDelay);
                await Task.Delay(hmrcAuthTokenBrokerConfig.RetryDelay);
            }
        }

        return tempToken;
    }

    private async Task<string> GetPrivilegedAccessTokenAsync()
    {
        logger.LogDebug("Retrieving privileged access token");
        var secret = "secret"; // await secretRepository.GetSecretAsync(PrivilegedAccessSecretName);
        var privilegedToken = totpService.Generate(secret);
        logger.LogDebug("Privileged access token retrieved successfully");
        return privilegedToken;
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeCancellationTokenAsync();
    }

    private async ValueTask DisposeCancellationTokenAsync()
    {
        if (_cancellationTokenSource != null)
        {
            await _cancellationTokenSource.CancelAsync();
            _cancellationTokenSource.Dispose();
        }

        _cancellationTokenSource = null;
        await Task.CompletedTask;
    }
}