using System;
using System.Threading.Tasks;
using Moq;
using NLog;
using NUnit.Framework;
using SFA.DAS.TokenService.Application.PrivilegedAccess.GetPrivilegedAccessToken;
using SFA.DAS.TokenService.Domain;
using SFA.DAS.TokenService.Domain.Data;
using SFA.DAS.TokenService.Domain.Services;
using SFA.DAS.TokenService.Infrastructure.ExecutionPolicies;

namespace SFA.DAS.TokenService.Application.UnitTests.PrivilegedAccess.GetPrivilegedAccessToken.PrivilegedAccessQueryHandlerTests
{
    public class WhenHandlingPrivilegedAccessQuery
    {
        private const string PrivilegedAccessSecretName = "PrivilegedAccessSecret";
        private const string OgdSecret = "OGD-SECRET";
        private const string TotpCode = "TOTP-CODE";
        private const string AccessToken = "ACCESS-TOKEN";
        private const string RefreshToken = "REFRESH-TOKEN";
        private readonly DateTime ExpiresAt = DateTime.Now.AddHours(2);
        private const string Scope = "SCOPE";
        private const string TokenType = "TOKEN-TYPE";
        private const string CachedAccessToken = "CACHED-ACCESS-TOKEN";
        private const string CachedRefreshToken = "CACHED-REFRESH-TOKEN";
        private readonly DateTime CachedExpiresAt = DateTime.Now.AddHours(1);
        private const string CachedScope = "CACHED-SCOPE";
        private const string CachedTokenType = "CACHED-TOKEN-TYPE";
        private const string CacheKey = "OGD-TOKEN";
        private const string RefreshedAccessToken = "REFRESHED-ACCESS-TOKEN";
        private const string RefreshedRefreshToken = "REFRESHED-REFRESH-TOKEN";
        private readonly DateTime RefreshedExpiresAt = DateTime.Now.AddHours(3);
        private const string RefreshedScope = "REFRESHED-SCOPE";
        private const string RefreshedTokenType = "REFRESHED-TOKEN-TYPE";

        private Mock<ISecretRepository> _secretRepository;
        private Mock<ITotpService> _totpService;
        private Mock<IOAuthTokenService> _oauthTokenService;
        private Mock<ICacheProvider> _cacheProvider;
        private PrivilegedAccessQueryHandler _handler;
        private PrivilegedAccessQuery _query;
        private Mock<ILogger> _logger;

        [SetUp]
        public void Arrange()
        {
            _secretRepository = new Mock<ISecretRepository>();
            _secretRepository.Setup(r => r.GetSecretAsync(PrivilegedAccessSecretName))
                .ReturnsAsync(OgdSecret);

            _totpService = new Mock<ITotpService>();
            _totpService.Setup(g => g.Generate(OgdSecret))
                .Returns(TotpCode);

            _oauthTokenService = new Mock<IOAuthTokenService>();
            _oauthTokenService.Setup(s => s.GetAccessToken(TotpCode))
                .ReturnsAsync(new OAuthAccessToken
                {
                    AccessToken = AccessToken,
                    RefreshToken = RefreshToken,
                    ExpiresAt = ExpiresAt,
                    Scope = Scope,
                    TokenType = TokenType
                });
            _oauthTokenService.Setup(s => s.GetAccessTokenFromRefreshToken(TotpCode, CachedRefreshToken))
                .ReturnsAsync(new OAuthAccessToken
                {
                    AccessToken = RefreshedAccessToken,
                    RefreshToken = RefreshedRefreshToken,
                    ExpiresAt = RefreshedExpiresAt,
                    Scope = RefreshedScope,
                    TokenType = RefreshedTokenType
                });

            _cacheProvider = new Mock<ICacheProvider>();

            _logger = new Mock<ILogger>();

            _handler = new PrivilegedAccessQueryHandler(_secretRepository.Object, _totpService.Object, _oauthTokenService.Object, _cacheProvider.Object, _logger.Object, new NoopExecutionPolicy());

            _query = new PrivilegedAccessQuery();
        }

        [Test]
        public async Task ThenItShouldGetTheOgdSecretFromTheRepository()
        {
            // Act
            await _handler.Handle(_query);

            // Assert
            _secretRepository.Verify(r => r.GetSecretAsync(PrivilegedAccessSecretName), Times.Once);
        }

        [Test]
        public async Task ThenItShouldUseTheOgdSecretToProduceTotp()
        {
            // Act
            await _handler.Handle(_query);

            // Assert
            _totpService.Verify(s => s.Generate(OgdSecret), Times.Once);
        }

        [Test]
        public async Task ThenItShouldUseTheTotpToGetAccessCode()
        {
            // Act
            await _handler.Handle(_query);

            // Assert
            _oauthTokenService.Verify(s => s.GetAccessToken(TotpCode), Times.Once);
        }

        [Test]
        public async Task ThenItShouldReturnAccessCodeDetailsFromOAuthService()
        {
            // Act
            var actual = await _handler.Handle(_query);

            // Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(AccessToken, actual.AccessToken);
            Assert.AreEqual(RefreshToken, actual.RefreshToken);
            Assert.AreEqual(ExpiresAt, actual.ExpiresAt);
            Assert.AreEqual(Scope, actual.Scope);
            Assert.AreEqual(TokenType, actual.TokenType);
        }

        [Test]
        public async Task ThenItShouldStoreAccessCodeInCache()
        {
            // Act
            var actual = await _handler.Handle(_query);

            // Assert
            _cacheProvider.Verify(p => p.SetAsync(CacheKey, It.Is<OAuthAccessToken>(t => t.AccessToken == AccessToken), ExpiresAt), Times.Once);
        }

        [Test]
        public async Task ThenItShouldReturnAccessCodeDetailsFromCacheIfAvailable()
        {
            // Arrange
            _cacheProvider.Setup(p => p.GetAsync(CacheKey))
                .ReturnsAsync(new OAuthAccessToken
                {
                    AccessToken = CachedAccessToken,
                    RefreshToken = CachedRefreshToken,
                    ExpiresAt = CachedExpiresAt,
                    Scope = CachedScope,
                    TokenType = CachedTokenType
                });

            // Act
            var actual = await _handler.Handle(_query);

            // Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(CachedAccessToken, actual.AccessToken);
            Assert.AreEqual(CachedRefreshToken, actual.RefreshToken);
            Assert.AreEqual(CachedExpiresAt, actual.ExpiresAt);
            Assert.AreEqual(CachedScope, actual.Scope);
            Assert.AreEqual(CachedTokenType, actual.TokenType);
        }

        [Test]
        public async Task ThenItShouldAttemptToUseRefreshCodeToGetNewAccessCodeIfCodeExpired()
        {
            // Arrange
            _cacheProvider.Setup(p => p.GetAsync(CacheKey))
                .ReturnsAsync(new OAuthAccessToken
                {
                    AccessToken = CachedAccessToken,
                    RefreshToken = CachedRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddHours(-1),
                    Scope = CachedScope,
                    TokenType = CachedTokenType
                });

            // Act
            var actual = await _handler.Handle(_query);

            // Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(RefreshedAccessToken, actual.AccessToken);
            Assert.AreEqual(RefreshedRefreshToken, actual.RefreshToken);
            Assert.AreEqual(RefreshedExpiresAt, actual.ExpiresAt);
            Assert.AreEqual(RefreshedScope, actual.Scope);
            Assert.AreEqual(RefreshedTokenType, actual.TokenType);
        }

    }
}
