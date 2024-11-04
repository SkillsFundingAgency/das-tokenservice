using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.TokenService.Application.PrivilegedAccess.GetPrivilegedAccessToken;
using SFA.DAS.TokenService.Application.PrivilegedAccess.TokenRefresh;
using SFA.DAS.TokenService.Domain;
using SFA.DAS.TokenService.Domain.Data;
using SFA.DAS.TokenService.Domain.Services;

namespace SFA.DAS.TokenService.Application.UnitTests.PrivilegedAccess.GetPrivilegedAccessToken.PrivilegedAccessQueryHandlerTests;

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
    
    private const string CachedRefreshToken = "CACHED-REFRESH-TOKEN";
    private const string RefreshedAccessToken = "REFRESHED-ACCESS-TOKEN";
    private const string RefreshedRefreshToken = "REFRESHED-REFRESH-TOKEN";
    private readonly DateTime _refreshedExpiresAt = DateTime.Now.AddHours(3);
    private const string RefreshedScope = "REFRESHED-SCOPE";
    private const string RefreshedTokenType = "REFRESHED-TOKEN-TYPE";

    private Mock<ISecretRepository> _secretRepository;
    private Mock<ITotpService> _totpService;
    private Mock<IOAuthTokenService> _oauthTokenService;
    private PrivilegedAccessQueryHandler _handler;
    private PrivilegedAccessQuery _query;
    private Mock<ILogger<HmrcAuthTokenBroker>> _logger;
    private Mock<ITokenRefresher> _tokenRefresher;
    private Mock<IHmrcAuthTokenBrokerConfig> _hmrcAuthBrokenConfig;

    [SetUp]
    public void Arrange()
    {
        _secretRepository = new Mock<ISecretRepository>();
        _secretRepository.Setup(r => r.GetSecretAsync(PrivilegedAccessSecretName))
            .ReturnsAsync(OgdSecret);

        _totpService = new Mock<ITotpService>();
        _totpService.Setup(g => g.Generate(OgdSecret))
            .Returns(TotpCode);

        _hmrcAuthBrokenConfig = new Mock<IHmrcAuthTokenBrokerConfig>();
        _hmrcAuthBrokenConfig.Setup(config => config.RetryDelay).Returns(TimeSpan.FromSeconds(0));

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
        _oauthTokenService.Setup(s => s.GetAccessToken(TotpCode, CachedRefreshToken))
            .ReturnsAsync(new OAuthAccessToken
            {
                AccessToken = RefreshedAccessToken,
                RefreshToken = RefreshedRefreshToken,
                ExpiresAt = _refreshedExpiresAt,
                Scope = RefreshedScope,
                TokenType = RefreshedTokenType
            });

        _logger = new Mock<ILogger<HmrcAuthTokenBroker>>();

        _tokenRefresher = new Mock<ITokenRefresher>();

        var hmrcAuthTokenContainer = new HmrcAuthTokenBroker(
            new NoopExecutionPolicy(),
            _logger.Object,
            _oauthTokenService.Object,
            _secretRepository.Object,
            _totpService.Object,
            _tokenRefresher.Object,
            _hmrcAuthBrokenConfig.Object
        );

        _handler = new PrivilegedAccessQueryHandler(hmrcAuthTokenContainer);

        _query = new PrivilegedAccessQuery();
    }

    [Test]
    public async Task ThenItShouldGetTheOgdSecretFromTheRepository()
    {
        // Act
        await _handler.Handle(_query, CancellationToken.None);

        // Assert
        _secretRepository.Verify(r => r.GetSecretAsync(PrivilegedAccessSecretName), Times.Once);
    }

    [Test]
    public async Task ThenItShouldUseTheOgdSecretToProduceTotp()
    {
        // Act
        await _handler.Handle(_query, CancellationToken.None);

        // Assert
        _totpService.Verify(s => s.Generate(OgdSecret), Times.Once);
    }

    [Test]
    public async Task ThenItShouldUseTheTotpToGetAccessCode()
    {
        // Act
        await _handler.Handle(_query, CancellationToken.None);

        // Assert
        _oauthTokenService.Verify(s => s.GetAccessToken(TotpCode), Times.Once);
    }

    [Test]
    public async Task ThenItShouldReturnAccessCodeDetailsFromOAuthService()
    {
        // Act
        var actual = await _handler.Handle(_query, CancellationToken.None);

        // Assert
        actual!.Should().NotBeNull();
        actual!.AccessToken.Should().Be(AccessToken);
        actual.RefreshToken.Should().Be(RefreshToken);
        actual.ExpiresAt.Should().Be(ExpiresAt);
        actual.Scope.Should().Be(Scope);
        actual.TokenType.Should().Be(TokenType);
    }
}