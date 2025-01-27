using Microsoft.Extensions.Logging;
using SFA.DAS.TokenService.Domain;
using SFA.DAS.TokenService.Domain.Data;
using SFA.DAS.TokenService.Domain.Services;
using SFA.DAS.TokenService.Infrastructure.ExecutionPolicies;

namespace SFA.DAS.TokenService.Application.PrivilegedAccess.TokenRefresh;

public sealed class HmrcAuthTokenBroker(
    [RequiredPolicy(HmrcExecutionPolicy.Name)] ExecutionPolicy executionPolicy,
    ILogger<HmrcAuthTokenBroker> logger,
    IOAuthTokenService tokenService,
    ISecretRepository secretRepository,
    ITotpService totpService,
    ITokenRefresher tokenRefresher,
    IHmrcAuthTokenBrokerConfig config)
    : IHmrcAuthTokenBroker, IDisposable
{
    private const string PrivilegedAccessSecretName = "PrivilegedAccessSecret";

    private Task<OAuthAccessToken?>? _initializationTask;
    private CancellationTokenSource? _cancellationTokenSource;

    public async Task<OAuthAccessToken?> GetTokenAsync()
    {
        // Ensure only one initialization task is created and awaited.
        _initializationTask ??= InitializeTokenAsync();
        return await _initializationTask;
    }

    private async Task<OAuthAccessToken?> InitializeTokenAsync()
    {
        var token = await RetrieveTokenAsync();
        StartTokenBackgroundRefresh(token);
        return token;
    }

    private void StartTokenBackgroundRefresh(OAuthAccessToken? token)
    {
        DisposeCancellationToken();

        if (token == null)
        {
            logger.LogWarning("Cannot start background refresh; token is null.");
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        _ = tokenRefresher.StartTokenBackgroundRefreshAsync(
            token,
            RefreshTokenAsync,
            _cancellationTokenSource.Token
        );
    }

    private async Task<OAuthAccessToken?> RefreshTokenAsync(OAuthAccessToken existingToken)
    {
        try
        {
            logger.LogInformation("Refreshing token (expired at {ExpiresAt})", existingToken.ExpiresAt);

            var refreshedToken = await GetTokenUsingRefreshTokenAsync(existingToken)
                                ?? await RetrieveTokenAsync();

            logger.LogInformation("Token refresh completed (new expiry {Expiry})", refreshedToken?.ExpiresAt);
            return refreshedToken;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while refreshing token.");
            return null;
        }
    }

    private async Task<OAuthAccessToken?> GetTokenUsingRefreshTokenAsync(OAuthAccessToken token)
    {
        try
        {
            var privilegedAccessToken = await GeneratePrivilegedAccessTokenAsync();
            return await executionPolicy.ExecuteAsync(() =>
                tokenService.GetAccessToken(privilegedAccessToken, token.RefreshToken!));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to refresh token using refresh token.");
            return null;
        }
    }

    private async Task<OAuthAccessToken?> RetrieveTokenAsync()
    {
        while (true)
        {
            try
            {
                logger.LogDebug("Requesting new token...");

                var privilegedAccessToken = await GeneratePrivilegedAccessTokenAsync();
                var token = await executionPolicy.ExecuteAsync(() =>
                    tokenService.GetAccessToken(privilegedAccessToken));

                if (token != null)
                {
                    logger.LogInformation("Token successfully retrieved.");
                    return token;
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to retrieve token; retrying...");
            }

            logger.LogWarning("Retrying after {RetryDelay}ms...", config.RetryDelay);
            await Task.Delay(config.RetryDelay);
        }
    }

    private async Task<string> GeneratePrivilegedAccessTokenAsync()
    {
        logger.LogDebug("Generating privileged access token...");

        var secret = await secretRepository.GetSecretAsync(PrivilegedAccessSecretName);
        var token = totpService.Generate(secret);

        logger.LogDebug("Privileged access token generated.");
        return token;
    }

    public void Dispose()
    {
        DisposeCancellationToken();
    }

    private void DisposeCancellationToken()
    {
        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }
    }
}
