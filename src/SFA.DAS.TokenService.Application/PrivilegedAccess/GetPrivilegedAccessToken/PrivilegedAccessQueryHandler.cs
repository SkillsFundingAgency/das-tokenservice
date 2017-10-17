using System;
using System.Threading.Tasks;
using MediatR;
using NLog;
using SFA.DAS.TokenService.Domain;
using SFA.DAS.TokenService.Domain.Data;
using SFA.DAS.TokenService.Domain.Services;
using SFA.DAS.TokenService.Infrastructure.ExecutionPolicies;

namespace SFA.DAS.TokenService.Application.PrivilegedAccess.GetPrivilegedAccessToken
{
    public class PrivilegedAccessQueryHandler : IAsyncRequestHandler<PrivilegedAccessQuery, OAuthAccessToken>
    {
        private const string PrivilegedAccessSecretName = "PrivilegedAccessSecret";
        private const string CacheKey = "OGD-TOKEN";

        private readonly ISecretRepository _secretRepository;
        private readonly ITotpService _totpService;
        private readonly IOAuthTokenService _tokenService;
        private readonly ICacheProvider _cacheProvider;
        private readonly ILogger _logger;
        private readonly ExecutionPolicy _executionPolicy;

        public PrivilegedAccessQueryHandler(ISecretRepository secretRepository,
                                            ITotpService totpService,
                                            IOAuthTokenService tokenService,
                                            ICacheProvider cacheProvider,
                                            ILogger logger,
            [RequiredPolicy(HmrcExecutionPolicy.Name)] ExecutionPolicy executionPolicy)
        {
            _secretRepository = secretRepository;
            _totpService = totpService;
            _tokenService = tokenService;
            _cacheProvider = cacheProvider;
            _logger = logger;
            _executionPolicy = executionPolicy;
        }

        public async Task<OAuthAccessToken> Handle(PrivilegedAccessQuery message)
        {
            var accessToken = await GetToken();
            if (accessToken.ExpiresAt <= DateTime.UtcNow)
            {
                accessToken = await RefreshToken(accessToken.RefreshToken);
            }
            return accessToken;
        }

        private async Task<OAuthAccessToken> GetToken()
        {
            var accessToken = await GetTokenFromCache();
            if (accessToken == null)
            {
                accessToken = await GetTokenFromService();
                await _cacheProvider.SetAsync(CacheKey, accessToken, accessToken.ExpiresAt);
            }
            return accessToken;
        }
        private async Task<OAuthAccessToken> RefreshToken(string refreshToken)
        {
            var accessToken = await GetTokenFromServiceUsingRefreshToken(refreshToken);
            if (accessToken == null)
            {
                accessToken = await GetTokenFromService();
            }
            await _cacheProvider.SetAsync(CacheKey, accessToken, accessToken.ExpiresAt);
            return accessToken;
        }

        private async Task<OAuthAccessToken> GetTokenFromCache()
        {
            _logger.Debug("Attempting to get privileged access token from cache");
            var token = (OAuthAccessToken)await _cacheProvider.GetAsync(CacheKey);
            return token;
        }
        private async Task<OAuthAccessToken> GetTokenFromService()
        {
            _logger.Debug("Attempting to get privileged access token from service");
            var secret = await _secretRepository.GetSecretAsync(PrivilegedAccessSecretName);
            var totp = _totpService.Generate(secret);
            var token = await _executionPolicy.ExecuteAsync(async () => await _tokenService.GetAccessToken(totp));

            _logger.Debug("Got privileged access token from service");
            return token;
        }
        private async Task<OAuthAccessToken> GetTokenFromServiceUsingRefreshToken(string refreshToken)
        {
            try
            {
                _logger.Debug("Attempting to get privileged access token from service using refresh token");
                var secret = await _secretRepository.GetSecretAsync(PrivilegedAccessSecretName);
                var totp = _totpService.Generate(secret);
                var token = await _executionPolicy.ExecuteAsync(async () => await _tokenService.GetAccessTokenFromRefreshToken(totp, refreshToken));

                _logger.Debug("Got privileged access token from service using refresh token");
                return token;
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, $"Error trying to refresh access token - {ex.Message}");
                return null;
            }
        }
    }
}