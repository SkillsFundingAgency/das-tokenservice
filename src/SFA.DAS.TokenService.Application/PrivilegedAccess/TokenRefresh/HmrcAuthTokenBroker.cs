using Microsoft.Extensions.Logging;
using SFA.DAS.TokenService.Domain;
using SFA.DAS.TokenService.Domain.Data;
using SFA.DAS.TokenService.Domain.Services;
using SFA.DAS.TokenService.Infrastructure.ExecutionPolicies;

namespace SFA.DAS.TokenService.Application.PrivilegedAccess.TokenRefresh;

public sealed class HmrcAuthTokenBroker : IHmrcAuthTokenBroker, IDisposable
{
    private const string PrivilegedAccessSecretName = "PrivilegedAccessSecret";

    private readonly ExecutionPolicy _executionPolicy;
    private readonly ILogger<HmrcAuthTokenBroker> _logger;
    private readonly IOAuthTokenService _tokenService;
    private readonly ISecretRepository _secretRepository;
    private readonly ITotpService _totpService;
    private readonly ITokenRefresher _tokenRefresher;
    private readonly IHmrcAuthTokenBrokerConfig _config;

    private OAuthAccessToken? _cachedAccessToken;
    private CancellationTokenSource? _cancellationTokenSource;

    public HmrcAuthTokenBroker(
        [RequiredPolicy(HmrcExecutionPolicy.Name)] ExecutionPolicy executionPolicy,
        ILogger<HmrcAuthTokenBroker> logger,
        IOAuthTokenService tokenService,
        ISecretRepository secretRepository,
        ITotpService totpService,
        ITokenRefresher tokenRefresher,
        IHmrcAuthTokenBrokerConfig config)
    {
        _executionPolicy = executionPolicy;
        _logger = logger;
        _tokenService = tokenService;
        _secretRepository = secretRepository;
        _totpService = totpService;
        _tokenRefresher = tokenRefresher;
        _config = config;

        _ = InitializeTokenAsync();
    }

    public async Task<OAuthAccessToken?> GetTokenAsync()
    {
        if (_cachedAccessToken == null)
        {
            _logger.LogDebug("Token not initialized; initializing...");
            _cachedAccessToken = await InitializeTokenAsync();
        }

        return _cachedAccessToken;
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
            _logger.LogWarning("Cannot start background refresh; token is null.");
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        _ = _tokenRefresher.StartTokenBackgroundRefreshAsync(
            token, 
            RefreshTokenAsync, 
            _cancellationTokenSource.Token
        );
    }

    private async Task<OAuthAccessToken?> RefreshTokenAsync(OAuthAccessToken existingToken)
    {
        try
        {
            _logger.LogInformation("Refreshing token (expired at {ExpiresAt})", existingToken.ExpiresAt);

            var refreshedToken = await GetTokenUsingRefreshTokenAsync(existingToken) 
                                ?? await RetrieveTokenAsync();

            _cachedAccessToken = refreshedToken;

            _logger.LogInformation("Token refresh completed (new expiry {Expiry})", refreshedToken?.ExpiresAt);
            return refreshedToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while refreshing token.");
            return null;
        }
    }

    private async Task<OAuthAccessToken?> GetTokenUsingRefreshTokenAsync(OAuthAccessToken token)
    {
        try
        {
            var privilegedAccessToken = await GeneratePrivilegedAccessTokenAsync();
            return await _executionPolicy.ExecuteAsync(() =>
                _tokenService.GetAccessToken(privilegedAccessToken, token.RefreshToken!));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to refresh token using refresh token.");
            return null;
        }
    }

    private async Task<OAuthAccessToken?> RetrieveTokenAsync()
    {
        while (true)
        {
            try
            {
                _logger.LogDebug("Requesting new token...");

                var privilegedAccessToken = await GeneratePrivilegedAccessTokenAsync();
                var token = await _executionPolicy.ExecuteAsync(() =>
                    _tokenService.GetAccessToken(privilegedAccessToken));

                if (token != null)
                {
                    _logger.LogInformation("Token successfully retrieved.");
                    return token;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve token; retrying...");
            }

            _logger.LogWarning("Retrying after {RetryDelay}ms...", _config.RetryDelay);
            await Task.Delay(_config.RetryDelay);
        }
    }

    private async Task<string> GeneratePrivilegedAccessTokenAsync()
    {
        _logger.LogDebug("Generating privileged access token...");

        var secret = await _secretRepository.GetSecretAsync(PrivilegedAccessSecretName);
        var token = _totpService.Generate(secret);

        _logger.LogDebug("Privileged access token generated.");
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
