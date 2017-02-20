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

        private readonly ISecretRepository _secretRepository;
        private readonly ITotpService _totpService;
        private readonly IOAuthTokenService _tokenService;

        public PrivilegedAccessQueryHandler(ISecretRepository secretRepository, ITotpService totpService, IOAuthTokenService tokenService)
        {
            _secretRepository = secretRepository;
            _totpService = totpService;
            _tokenService = tokenService;
        }

        public async Task<OAuthAccessToken> Handle(PrivilegedAccessQuery message)
        {
            // Get the secret
            // Make totp from secret
            // Get access token from token service
            var secret = await _secretRepository.GetSecretAsync(PrivilegedAccessSecretName);
            var totp = _totpService.Generate(secret);
            var accessCode = await _tokenService.GetAccessToken(totp);

            return accessCode;
        }
    }
}