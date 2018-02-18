using System;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using SFA.DAS.TokenService.Domain;
using SFA.DAS.TokenService.Domain.Data;
using SFA.DAS.TokenService.Domain.Services;
using SFA.DAS.TokenService.Infrastructure.ExecutionPolicies;

namespace SFA.DAS.TokenService.Application.PrivilegedAccess.TokenRefresh
{
    public sealed class HmrcAuthTokenBroker : IHmrcAuthTokenBroker, IDisposable
    {
        private const string PrivilegedAccessSecretName = "PrivilegedAccessSecret";

        private readonly ExecutionPolicy _executionPolicy;
        private readonly ILogger _logger;
        private readonly IOAuthTokenService _tokenService;
        private readonly ISecretRepository _secretRepository;
        private readonly ITotpService _totpService;
        private readonly ITokenRefresher _tokenRefresher;

        private readonly Task<OAuthAccessToken> _initialiseTask;
        private OAuthAccessToken _cachedAccessToken;

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public HmrcAuthTokenBroker(
            [RequiredPolicy(HmrcExecutionPolicy.Name)] ExecutionPolicy executionPolicy, 
            ILogger logger,
            IOAuthTokenService tokenService,
            ISecretRepository secretRepository,
            ITotpService totpService,
            ITokenRefresher tokenRefresher)
        {
            _secretRepository = secretRepository;
            _totpService = totpService;
            _tokenService = tokenService;
            _logger = logger;
            _executionPolicy = executionPolicy;
            _tokenRefresher = tokenRefresher;
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
            _cancellationTokenSource.Cancel();
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
                _logger.Debug($"Refreshing token (expired {token.ExpiresAt})");
                var privilegedAccessToken = await GetPrivilegedAccessToken();
                var newToken = await _executionPolicy.ExecuteAsync(async () => await _tokenService.GetAccessTokenFromRefreshToken(privilegedAccessToken, token.RefreshToken));
                return newToken;
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, $"Error trying to refresh access token - {ex.Message}");
                return null;
            }
        }

        private async Task<OAuthAccessToken> GetTokenFromServiceAsync()
        {
            var privilegedAccessToken = await GetPrivilegedAccessToken();
            var tempToken = await _executionPolicy.ExecuteAsync(async () => await _tokenService.GetAccessToken(privilegedAccessToken));
            if(tempToken != null)
            {
                _cachedAccessToken = tempToken;
            }
            return _cachedAccessToken;
        }

        private async Task<string> GetPrivilegedAccessToken()
        {
            _logger.Debug("Attempting to get privileged access token from service using refresh token");
            var secret = await _secretRepository.GetSecretAsync(PrivilegedAccessSecretName);
            var privilegedToken = _totpService.Generate(secret);
            _logger.Debug("Attempting to get privileged access token from service using refresh token");
            return privilegedToken;
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
    }
}