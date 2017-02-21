using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.TokenService.Application.PrivilegedAccess.GetPrivilegedAccessToken;
using SFA.DAS.TokenService.Domain.Data;
using SFA.DAS.TokenService.Domain.Services;

namespace SFA.DAS.TokenService.Application.UnitTests.PrivilegedAccess.GetPrivilegedAccessToken.PrivilegedAccessQueryHandlerTests
{
    public class WhenHandlingPrivilegedAccessQuery
    {
        private const string PrivilegedAccessSecretName = "PrivilegedAccessSecret";
        private const string OgdSecret = "OGD-SECRET";
        private const string TotpCode = "TOTP-CODE";
        private const string AccessToken = "ACCESS-TOKEN";
        private const string RefreshToken = "REFRESH-TOKEN";
        private const int ExpiresIn = 123;
        private const string Scope = "SCOPE";
        private const string TokenType = "TOKEN-TYPE";

        private Mock<ISecretRepository> _secretRepository;
        private Mock<ITotpService> _totpService;
        private Mock<IOAuthTokenService> _oauthTokenService;
        private PrivilegedAccessQueryHandler _handler;
        private PrivilegedAccessQuery _query;

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
                .ReturnsAsync(new Domain.OAuthAccessToken
                {
                    AccessToken = AccessToken,
                    RefreshToken = RefreshToken,
                    ExpiresIn = ExpiresIn,
                    Scope = Scope,
                    TokenType = TokenType
                });

            _handler = new PrivilegedAccessQueryHandler(_secretRepository.Object, _totpService.Object, _oauthTokenService.Object);

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
            Assert.AreEqual(ExpiresIn, actual.ExpiresIn);
            Assert.AreEqual(Scope, actual.Scope);
            Assert.AreEqual(TokenType, actual.TokenType);
        }
    }
}
