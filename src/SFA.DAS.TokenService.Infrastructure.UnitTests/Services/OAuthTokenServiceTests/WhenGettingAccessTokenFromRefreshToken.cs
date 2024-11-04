using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.TokenService.Infrastructure.Configuration;
using SFA.DAS.TokenService.Infrastructure.Http;
using SFA.DAS.TokenService.Infrastructure.Services;

namespace SFA.DAS.TokenService.Infrastructure.UnitTests.Services.OAuthTokenServiceTests;

public class WhenGettingAccessTokenFromRefreshToken
{
    private const string AccessToken = "ACCESS-TOKEN";
    private const string RefreshToken = "REFRESH-TOKEN";
    private const int ExpiresIn = 123;
    private const string Scopes = "SCOPES";
    private const string TokenType = "TOKEN-TYPE";
    private const string ClientSecret = "SECRET";
    private const string ClientId = "CLIENT-ID";
    private const string ExistingRefreshToken = "ORIG-REFRESH-TOKEN";

    private Mock<IHttpClientWrapper> _httpClient;
    private OAuthTokenServiceConfiguration _configuration;
    private OAuthTokenService _service;

    [SetUp]
    public void Arrange()
    {
        _configuration = new OAuthTokenServiceConfiguration
        {
            Url = "http://unit.test/token",
            ClientId = ClientId
        };

        _httpClient = new Mock<IHttpClientWrapper>();
        _httpClient.Setup(c => c.Post<OAuthTokenResponse>(_configuration.Url, It.IsAny<OAuthTokenRequest>()))
            .ReturnsAsync(new OAuthTokenResponse
            {
                AccessToken = AccessToken,
                RefreshToken = RefreshToken,
                ExpiresIn = ExpiresIn,
                Scope = Scopes,
                TokenType = TokenType
            });

        _service = new OAuthTokenService(_httpClient.Object, _configuration, Mock.Of<ILogger<OAuthTokenService>>());
    }

    [Test]
    public async Task ThenItShouldReturnTheAccessTokenFromTheHttpClient()
    {
        // Act
        var actual = await _service.GetAccessToken(ClientSecret, ExistingRefreshToken);

        // Assert
        actual.Should().NotBeNull();
        actual.AccessToken.Should().Be(AccessToken);
        actual.RefreshToken.Should().Be(RefreshToken);
        actual.Scope.Should().Be(Scopes);
        actual.TokenType.Should().Be(TokenType);
    }

    [Test]
    public async Task ThenItShouldReturnExpiresAtAsUtfNowPlusExpiresInSeconds()
    {
        // Act
        var actual = await _service.GetAccessToken(ClientSecret, ExistingRefreshToken);

        // Assert
        var expectedExpiry = DateTime.UtcNow.AddSeconds(ExpiresIn);
        (actual.ExpiresAt >= expectedExpiry.AddSeconds(-1)).Should().BeTrue($"Expected Expiry time to be greater than or equal to {expectedExpiry.AddSeconds(-1)}, but was {actual.ExpiresAt}");
        (actual.ExpiresAt <= expectedExpiry.AddSeconds(1)).Should().BeTrue($"Expected Expiry time to be less than or equal to {expectedExpiry.AddSeconds(1)}, but was {actual.ExpiresAt}");
    }

    [Test]
    public async Task ThenItShouldUseCorrectRequestParameters()
    {
        // Act
        await _service.GetAccessToken(ClientSecret, ExistingRefreshToken);

        // Assert
        _httpClient.Verify(c => c.Post<OAuthTokenResponse>(_configuration.Url, 
            It.Is<OAuthTokenRequest>(r => r.ClientId == ClientId
                                                 && r.ClientSecret == ClientSecret
                                                 && r.RefreshToken == ExistingRefreshToken)), Times.Once);
    }
}