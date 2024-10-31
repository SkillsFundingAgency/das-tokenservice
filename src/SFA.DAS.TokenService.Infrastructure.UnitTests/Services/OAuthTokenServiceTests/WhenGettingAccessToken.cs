using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Moq;
using NLog;
using NUnit.Framework;
using SFA.DAS.TokenService.Infrastructure.Configuration;
using SFA.DAS.TokenService.Infrastructure.Http;
using SFA.DAS.TokenService.Infrastructure.Services;

namespace SFA.DAS.TokenService.Infrastructure.UnitTests.Services.OAuthTokenServiceTests
{
    public class WhenGettingAccessToken
    {
        private const string AccessToken = "ACCESS-TOKEN";
        private const string RefreshToken = "REFRESH-TOKEN";
        private const int ExpiresIn = 123;
        private const string Scopes = "SCOPES";
        private const string TokenType = "TOKEN-TYPE";
        private const string ClientSecret = "SECRET";
        private const string ClientId = "CLIENT-ID";

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

            _clientAcceptHeaders = new List<MediaTypeWithQualityHeaderValue>();
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

            _service = new OAuthTokenService(_httpClient.Object, _configuration, Mock.Of<ILogger>());
        }

        [Test]
        public async Task ThenItShouldReturnTheAccessTokenFromTheHttpClient()
        {
            // Act
            var actual = await _service.GetAccessToken(ClientSecret);

            // Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(AccessToken, actual.AccessToken);
            Assert.AreEqual(RefreshToken, actual.RefreshToken);
            Assert.AreEqual(Scopes, actual.Scope);
            Assert.AreEqual(TokenType, actual.TokenType);
        }

        [Test]
        public async Task ThenItShouldReturnExpiresAtAsUtfNowPlusExpiresInSeconds()
        {
            // Act
            var actual = await _service.GetAccessToken(ClientSecret);

            // Assert
            var expectedExpiry = DateTime.UtcNow.AddSeconds(ExpiresIn);
            Assert.IsTrue(actual.ExpiresAt >= expectedExpiry.AddSeconds(-1),
                $"Expected Expiry time to be greater than or equal to {expectedExpiry.AddSeconds(-1)}, but was {actual.ExpiresAt}");
            Assert.IsTrue(actual.ExpiresAt <= expectedExpiry.AddSeconds(1),
                $"Expected Expiry time to be less than or equal to {expectedExpiry.AddSeconds(1)}, but was {actual.ExpiresAt}");
        }

        [Test]
        public void ThenItShouldSetTheAcceptHeadersToHmrcJson()
        {
            // Assert
            Assert.AreEqual(1, _clientAcceptHeaders.Count);
            Assert.AreEqual("application/vnd.hmrc.1.0+json", _clientAcceptHeaders[0].MediaType);
        }

        [Test]
        public async Task ThenItShouldUseCorrectRequestParameters()
        {
            // Act
            await _service.GetAccessToken(ClientSecret);

            // Assert
            _httpClient.Verify(c => c.Post<OAuthTokenResponse>(_configuration.Url, It.Is<OAuthTokenRequest>(r => r.ClientId == ClientId
                                                                                                              && r.ClientSecret == ClientSecret
                                                                                                              && r.GrantType == "client_credentials"
                                                                                                              && r.Scopes == "read:apprenticeship-levy")), Times.Once);
        }
    }
}
