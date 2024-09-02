using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FluentAssertions;
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
    private List<MediaTypeWithQualityHeaderValue> _clientAcceptHeaders;
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

        _clientAcceptHeaders = [];
        _httpClient = new Mock<IHttpClientWrapper>();
        _httpClient.Setup(c => c.AcceptHeaders)
            .Returns(_clientAcceptHeaders);
        _httpClient.Setup(c => c.Post<OAuthTokenResponse>(_configuration.Url, It.IsAny<OAuthTokenRequest>()))
            .ReturnsAsync(new OAuthTokenResponse
            {
                AccessToken = AccessToken,
                RefreshToken = RefreshToken,
                ExpiresIn = ExpiresIn,
                Scope = Scopes,
                TokenType = TokenType
            });

        _service = new OAuthTokenService(_httpClient.Object, _configuration);
    }

    [Test]
    public async Task ThenItShouldReturnTheAccessTokenFromTheHttpClient()
    {
        // Act
        var actual = await _service.GetAccessTokenFromRefreshToken(ClientSecret, ExistingRefreshToken);

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
        var actual = await _service.GetAccessTokenFromRefreshToken(ClientSecret, ExistingRefreshToken);

        // Assert
        var expectedExpiry = DateTime.UtcNow.AddSeconds(ExpiresIn);
        (actual.ExpiresAt >= expectedExpiry.AddSeconds(-1)).Should().BeTrue($"Expected Expiry time to be greater than or equal to {expectedExpiry.AddSeconds(-1)}, but was {actual.ExpiresAt}");
        (actual.ExpiresAt <= expectedExpiry.AddSeconds(1)).Should().BeTrue($"Expected Expiry time to be less than or equal to {expectedExpiry.AddSeconds(1)}, but was {actual.ExpiresAt}");
    }

    [Test]
    public void ThenItShouldSetTheAcceptHeadersToHmrcJson()
    {
        // Assert
        _clientAcceptHeaders.Count.Should().Be(1);
        _clientAcceptHeaders[0].MediaType.Should().Be("application/vnd.hmrc.1.0+json" );
    }

    [Test]
    public async Task ThenItShouldUseCorrectRequestParameters()
    {
        // Act
        await _service.GetAccessTokenFromRefreshToken(ClientSecret, ExistingRefreshToken);

        // Assert
        _httpClient.Verify(c => c.Post<OAuthTokenResponse>(_configuration.Url, 
            It.Is<OAuthTokenRefreshRequest>(r => r.ClientId == ClientId
                                                 && r.ClientSecret == ClientSecret
                                                 && r.GrantType == "client_credentials"
                                                 && r.Scopes == "read:apprenticeship-levy"
                                                 && r.RefreshToken == ExistingRefreshToken)), Times.Once);
    }
}