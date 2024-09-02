﻿using Microsoft.Extensions.Logging;
using SFA.DAS.TokenService.Domain;
using SFA.DAS.TokenService.Domain.Data;
using SFA.DAS.TokenService.Domain.Services;
using SFA.DAS.TokenService.Infrastructure.ExecutionPolicies;

namespace SFA.DAS.TokenService.Application.PrivilegedAccess.TokenRefresh;

public sealed class HmrcAuthTokenBroker : IHmrcAuthTokenBroker, IDisposable
{
    private const string PrivilegedAccessSecretName = "PrivilegedAccessSecret";

    private readonly ExecutionPolicy _executionPolicy;
    private readonly ILogger _logger;
    private readonly IOAuthTokenService _tokenService;
    private readonly ISecretRepository _secretRepository;
    private readonly ITotpService _totpService;
    private readonly ITokenRefresher _tokenRefresher;
    private readonly IHmrcAuthTokenBrokerConfig _hmrcAuthTokenBrokerConfig;

    private readonly Task<OAuthAccessToken> _initialiseTask;
    private OAuthAccessToken _cachedAccessToken;

    private CancellationTokenSource _cancellationTokenSource;

    public HmrcAuthTokenBroker(
        [RequiredPolicy(HmrcExecutionPolicy.Name)] ExecutionPolicy executionPolicy, 
        ILogger logger,
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

    public async Task<OAuthAccessToken> GetTokenAsync()
    {
        await _initialiseTask;
        return _cachedAccessToken;
    }

    private Task<OAuthAccessToken> InitialiseToken()
    {
        return GetTokenFromServiceAsync()
            .ContinueWith((task) =>
            {
                StartTokenBackgroundRefresh(task.Result);
                return task.Result;
            });
    }

    private void StartTokenBackgroundRefresh(OAuthAccessToken token)
    {
        DisposeCancellationToken();
        _cancellationTokenSource = new CancellationTokenSource();
        _tokenRefresher.StartTokenBackgroundRefreshAsync(token, _cancellationTokenSource.Token, RefreshTokenAsync);
    }

    private async Task<OAuthAccessToken> RefreshTokenAsync(OAuthAccessToken existingToken)
    {
        var tempToken = await GetTokenFromServiceUsingRefreshTokenAsync(existingToken) ?? await GetTokenFromServiceAsync();
        if (tempToken != null)
        {
            _cachedAccessToken = tempToken;
        } 

        return _cachedAccessToken;
    }

    private async Task<OAuthAccessToken> GetTokenFromServiceUsingRefreshTokenAsync(OAuthAccessToken token)
    {
        try
        {
            _logger.LogDebug("Refreshing token (expired {ExpiresAt})", token.ExpiresAt);
            
            var privilegedAccessToken = await GetPrivilegedAccessToken();
            var newToken = await _executionPolicy.ExecuteAsync(async () => await _tokenService.GetAccessTokenFromRefreshToken(privilegedAccessToken, token.RefreshToken));
           
            _logger.LogDebug("Refresh token successful (new expiry {Expiry})", newToken?.ExpiresAt.ToString("yy-MMM-dd ddd HH:mm:ss") ?? "not available - new token is null");
            
            return newToken;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error trying to refresh access token.");
            return null;
        }
    }

    private Task<OAuthAccessToken> GetTokenFromServiceAsync()
    {
        return Task.Run(async () =>
        {
            var attempts = 0;

            OAuthAccessToken tempToken = null;
            while (tempToken == null)
            {
                _logger.LogDebug($"Initial call to get a token: attempt {++attempts}");

                var privilegedAccessToken = await GetPrivilegedAccessToken();
                tempToken = await _executionPolicy.ExecuteAsync(async () => await _tokenService.GetAccessToken(privilegedAccessToken));

                if (tempToken == null)
                {
                    _logger.LogWarning("The attempt to get a token from HMRC failed - sleeping {RetryDelay} and trying again", _hmrcAuthTokenBrokerConfig.RetryDelay);
                    await Task.Delay(_hmrcAuthTokenBrokerConfig.RetryDelay);
                }
            }

            _cachedAccessToken = tempToken;

            return _cachedAccessToken;
        });
    }

    private async Task<string> GetPrivilegedAccessToken()
    {
        _logger.LogDebug("Attempting to get privileged access token from service using refresh token");
        
        var secret = await _secretRepository.GetSecretAsync(PrivilegedAccessSecretName);
        var privilegedToken = _totpService.Generate(secret);
        
        _logger.LogDebug("Attempt to get privileged access token successfully");
        
        return privilegedToken;
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
        }

        _cancellationTokenSource = null;
    }
}