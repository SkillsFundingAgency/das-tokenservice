using System;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.TokenService.Domain;
using SFA.DAS.TokenService.Domain.Data;
using SFA.DAS.TokenService.Domain.Services;

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

        public PrivilegedAccessQueryHandler(ISecretRepository secretRepository,
                                            ITotpService totpService,
                                            IOAuthTokenService tokenService,
                                            ICacheProvider cacheProvider)
        {
            _secretRepository = secretRepository;
            _totpService = totpService;
            _tokenService = tokenService;
            _cacheProvider = cacheProvider;
        }

        public async Task<OAuthAccessToken> Handle(PrivilegedAccessQuery message)
        {
            var accessToken = await GetTokenFromCache();
            if (accessToken == null)
            {
                accessToken = await GetTokenFromService();
                await _cacheProvider.SetAsync(CacheKey, accessToken, accessToken.ExpiresAt);
            }
            return accessToken;
        }

        private async Task<OAuthAccessToken> GetTokenFromCache()
        {
            var token = (OAuthAccessToken)await _cacheProvider.GetAsync(CacheKey);
            if (token == null || token.ExpiresAt < DateTime.UtcNow)
            {
                return null;
            }
            return token;
        }
        private async Task<OAuthAccessToken> GetTokenFromService()
        {
            var secret = await _secretRepository.GetSecretAsync(PrivilegedAccessSecretName);
            var totp = _totpService.Generate(secret);
            return await _tokenService.GetAccessToken(totp);
        }
    }
}