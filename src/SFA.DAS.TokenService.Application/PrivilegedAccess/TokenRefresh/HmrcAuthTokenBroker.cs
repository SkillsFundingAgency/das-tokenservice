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
    private readonly IHmrcAuthTokenBrokerConfig _hmrcAuthTokenBrokerConfig;

    private readonly Task<OAuthAccessToken?> _initialiseTask;
    private OAuthAccessToken? _cachedAccessToken;

    private CancellationTokenSource? _cancellationTokenSource;

    public HmrcAuthTokenBroker(
        [RequiredPolicy(HmrcExecutionPolicy.Name)]
        ExecutionPolicy executionPolicy,
        ILogger<HmrcAuthTokenBroker> logger,
        IOAuthTokenService tokenService,
        ISecretRepository secretRepository,
        ITotpService totpService,
        ITokenRefresher tokenRefresher,
        IHmrcAuthTokenBrokerConfig hmrcAuthTokenBrokerConfig)
    {
        _secretRepository = secretRepository;
        _totpService = totpService;
        _tokenService = tokenService;
        _logger = logger;
        _executionPolicy = executionPolicy;
        _tokenRefresher = tokenRefresher;
        _hmrcAuthTokenBrokerConfig = hmrcAuthTokenBrokerConfig;
        _initialiseTask = InitialiseToken();
    }

    public async Task<OAuthAccessToken?> GetTokenAsync()
    {
        await _initialiseTask;
        return _cachedAccessToken;
    }

    private Task<OAuthAccessToken?> InitialiseToken()
    {
        return GetToken()
            .ContinueWith(task =>
            {
                StartTokenBackgroundRefresh(task.Result);
                return task.Result;
            });
    }

    private void StartTokenBackgroundRefresh(OAuthAccessToken? token)
    {
        DisposeCancellationToken();
        _cancellationTokenSource = new CancellationTokenSource();
        _tokenRefresher.StartTokenBackgroundRefreshAsync(token, RefreshToken, _cancellationTokenSource.Token);
    }

    private async Task<OAuthAccessToken?> RefreshToken(OAuthAccessToken existingToken)
    {
        var tempToken = await GetTokenUsingRefreshToken(existingToken) ?? await GetToken();
       
        if (tempToken != null)
        {
            _cachedAccessToken = tempToken;
        } 

        return _cachedAccessToken;
    }

    private async Task<OAuthAccessToken?> GetTokenUsingRefreshToken(OAuthAccessToken token)
    {
        try
        {
            _logger.LogInformation("Refreshing token (expired {ExpiresAt})", token.ExpiresAt);

            var privilegedAccessToken = await GeneratePrivilegedAccessToken();
            var newToken = await _executionPolicy.ExecuteAsync(async () => await _tokenService.GetAccessToken(privilegedAccessToken, token.RefreshToken!));

            _logger.LogInformation("Refresh token successful (new expiry {Expiry})", newToken?.ExpiresAt.ToString("yy-MMM-dd ddd HH:mm:ss") ?? "not available - new token is null");

            return newToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error trying to refresh access token.");
            return null;
        }
    }

    private async Task<OAuthAccessToken?> GetToken()
    {
        var attempts = 0;

        OAuthAccessToken? refreshedToken = null;

        while (refreshedToken == null)
        {
            _logger.LogInformation("Initial call to get a token: attempt {Attempts}", ++attempts);

            var privilegedAccessToken = await GeneratePrivilegedAccessToken();
            
            refreshedToken = await _executionPolicy.ExecuteAsync(async () => await _tokenService.GetAccessToken(privilegedAccessToken));

            if (refreshedToken != null)
            {
                continue;
            }

            _logger.LogWarning("The attempt to get a token from HMRC failed - sleeping {RetryDelay} and trying again", _hmrcAuthTokenBrokerConfig.RetryDelay);
            
            await Task.Delay(_hmrcAuthTokenBrokerConfig.RetryDelay);
        }

        _cachedAccessToken = refreshedToken;

        return _cachedAccessToken;
    }

    private async Task<string> GeneratePrivilegedAccessToken()
    {
        _logger.LogInformation("Attempting to generate privileged access token.");

        var secret = await _secretRepository.GetSecretAsync(PrivilegedAccessSecretName);
        var privilegedToken = _totpService.Generate(secret);

        _logger.LogInformation("Privileged access token generated successfully.");

        return privilegedToken;
    }

    public void Dispose()
    {
        DisposeCancellationToken();
    }

    private void DisposeCancellationToken()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
    }
}