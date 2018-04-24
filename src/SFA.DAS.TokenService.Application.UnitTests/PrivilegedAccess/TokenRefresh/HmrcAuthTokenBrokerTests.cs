using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.TokenService.Application.PrivilegedAccess.TokenRefresh;
using SFA.DAS.TokenService.Domain.Data;
using SFA.DAS.TokenService.Domain.Services;
using NLog;
using SFA.DAS.TokenService.Domain;
using SFA.DAS.TokenService.Infrastructure.ExecutionPolicies;

namespace SFA.DAS.TokenService.Application.UnitTests.PrivilegedAccess.TokenRefresh
{
    [TestFixture]
    public class HmrcAuthTokenBrokerTests 
    {
        [Test]
        public async Task GetTokenAsync_InitialRequestFails_ShouldStillGoOnToGetToken()
        {
            var fixtures = new HmrcAuthTokenBrokerTestFixtures()
                .WithInitialTaskResult(() => throw new HttpRequestException("Initial token request has failed"));

            var svc = fixtures.CreateHmrcAuthTokenBroker();

            var accessToken = await svc.GetTokenAsync();

            Assert.IsNotNull(accessToken);
        }

        [Test]
        public async Task GetTokenAsync_InitialThreeRequestFail_ShouldStillGoOnToGetToken()
        {
            var fixtures = new HmrcAuthTokenBrokerTestFixtures()
                .WithInitialTaskResult(() => throw new HttpRequestException("Initial token request has failed"))
                .WithInitialTaskResult(() => throw new HttpRequestException("Initial token request has failed"))
                .WithInitialTaskResult(() => null);

            var svc = fixtures.CreateHmrcAuthTokenBroker();

            var accessToken = await svc.GetTokenAsync();

            Assert.IsNotNull(accessToken);
        }

        [Test]
        public async Task GetTokenAsync_InitialThreeRequestFail_ShouldCallPostFourTimes()
        {
            var fixtures = new HmrcAuthTokenBrokerTestFixtures()
                .WithInitialTaskResult(() => throw new HttpRequestException("Initial token request has failed"))
                .WithInitialTaskResult(() => throw new HttpRequestException("Initial token request has failed"))
                .WithInitialTaskResult(() => null);

            var svc = fixtures.CreateHmrcAuthTokenBroker();

            var accessToken = await svc.GetTokenAsync();

            fixtures.OAuthTokenServiceMock.Verify(ots => ots.GetAccessToken(It.IsAny<string>()), Times.Exactly(4));
        }

        [Test]
        public async Task GetTokenAsync_WhenFirstRequestFails_ShouldCallPostTwice()
        {
            var fixtures = new HmrcAuthTokenBrokerTestFixtures()
                .WithInitialTaskResult(() => throw new HttpRequestException("Initial token request has failed"));

            var svc = fixtures.CreateHmrcAuthTokenBroker();

            var accessToken = await svc.GetTokenAsync();

            fixtures.OAuthTokenServiceMock.Verify(ots => ots.GetAccessToken(It.IsAny<string>()), Times.Exactly(2));
        }

        [Test]
        public async Task GetTokenAsync_WhenFirstSucceeds_ShouldReturnExpectedToken()
        {
            var accessToken = Guid.NewGuid().ToString();
            var refreshToken = Guid.NewGuid().ToString();

            var fixtures = new HmrcAuthTokenBrokerTestFixtures()
                .WithInitialTaskResult(() => new OAuthAccessToken
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                });

            var svc = fixtures.CreateHmrcAuthTokenBroker();

            var token = await svc.GetTokenAsync();

            Assert.AreEqual(accessToken, token.AccessToken, "Access token is not the expected value");
            Assert.AreEqual(refreshToken, token.RefreshToken, "Refresh token is not the expected value");
        }
    }

    public class HmrcAuthTokenBrokerTestFixtures
    {
        public HmrcAuthTokenBrokerTestFixtures()
        {
            LoggerMock = new Mock<ILogger>();
            OAuthTokenServiceMock = new Mock<IOAuthTokenService>();
            SecretRepositoryMock = new Mock<ISecretRepository>();
            TotpServiceMock = new Mock<ITotpService>();
            TokenRefresherMock = new Mock<ITokenRefresher>();
            HmrcAuthTokenBrokerConfigMock = new Mock<IHmrcAuthTokenBrokerConfig>();

            AccessTokenRequests = new Queue<Func<OAuthAccessToken>>();

            HmrcAuthTokenBrokerConfigMock.Setup(config => config.RetryDelay).Returns(TimeSpan.Zero);

            OAuthTokenServiceMock
                .Setup(ots => ots.GetAccessToken(It.IsAny<string>()))
                .Returns(() =>
                {
                    var func = AccessTokenRequests.Dequeue();
                    return Task.FromResult(func());
                });
        }

        public Mock<NLog.ILogger> LoggerMock { get; }
        public NLog.ILogger Logger => LoggerMock.Object;

        public Mock<IOAuthTokenService> OAuthTokenServiceMock { get; }
        public IOAuthTokenService OAuthTokenService => OAuthTokenServiceMock.Object;

        public Mock<ISecretRepository> SecretRepositoryMock { get;  }
        public ISecretRepository SecretRepository => SecretRepositoryMock.Object;

        public Mock<ITotpService> TotpServiceMock { get; }
        public ITotpService TotpService => TotpServiceMock.Object;

        public Mock<ITokenRefresher> TokenRefresherMock { get;  }
        public ITokenRefresher TokenRefresher => TokenRefresherMock.Object;

        public Mock<IHmrcAuthTokenBrokerConfig> HmrcAuthTokenBrokerConfigMock { get; set; }
        public IHmrcAuthTokenBrokerConfig HmrcAuthTokenBrokerConfig => HmrcAuthTokenBrokerConfigMock.Object;

        public Queue<Func<OAuthAccessToken>> AccessTokenRequests { get; }

        public HmrcAuthTokenBrokerTestFixtures WithInitialTaskResult(Func<OAuthAccessToken> getter)
        {
            AccessTokenRequests.Enqueue(getter);
            return this;
        }

        public HmrcAuthTokenBroker CreateHmrcAuthTokenBroker()
        {
            // Make sure that the queue is terminated with a 
            WithInitialTaskResult(() => new OAuthAccessToken
            {
                AccessToken = "AccessToken",
                ExpiresAt = DateTime.Now.AddMinutes(30),
                RefreshToken = "RefreshToken"
            });

            return new HmrcAuthTokenBroker(
                new HmrcExecutionPolicy(Logger), 
                Logger,
                OAuthTokenService, 
                SecretRepository,
                TotpService,
                TokenRefresher,
                HmrcAuthTokenBrokerConfig
                );
        }
    }
}
