using System;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.TokenService.Api.Types;

namespace SFA.DAS.TokenService.Api.Client.UnitTests.TokenServiceApiClientTests
{
    public class WhenGettingPrivilegedAccessToken
    {
        private const string ApiBaseUrl = "http://unit.tests";
        private const string AccessCode = "ACCESS-CODE";
        private readonly DateTime ExpiryTime = new DateTime(2017, 2, 22, 20, 34, 12);

        private Mock<ITokenServiceApiClientConfiguration> _configuration;
        private Mock<ISecureHttpClient> _httpClient;
        private TokenServiceApiClient _client;

        [SetUp]
        public void Arrange()
        {
            _configuration = new Mock<ITokenServiceApiClientConfiguration>();
            _configuration.Setup(c => c.ApiBaseUrl).Returns(ApiBaseUrl);

            _httpClient = new Mock<ISecureHttpClient>();
            _httpClient.Setup(c => c.GetAsync($"{ApiBaseUrl}/api/PrivilegedAccess"))
                .ReturnsAsync(JsonConvert.SerializeObject(new PrivilegedAccessToken
                {
                    AccessCode = AccessCode,
                    ExpiryTime = ExpiryTime
                }));

            _client = new TokenServiceApiClient(_configuration.Object, _httpClient.Object);
        }

        [Test]
        public async Task ThenItShouldReturnAccessToken()
        {
            // Act
            var actual = await _client.GetPrivilegedAccessTokenAsync();

            // Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(AccessCode, actual.AccessCode);
            Assert.AreEqual(ExpiryTime, actual.ExpiryTime);
        }
    }
}
