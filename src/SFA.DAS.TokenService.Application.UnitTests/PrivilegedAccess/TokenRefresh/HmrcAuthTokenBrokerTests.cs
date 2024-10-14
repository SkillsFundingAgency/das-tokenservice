using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.TokenService.Application.PrivilegedAccess.TokenRefresh;
using SFA.DAS.TokenService.Domain.Data;
using SFA.DAS.TokenService.Domain.Services;
using SFA.DAS.TokenService.Domain;
using SFA.DAS.TokenService.Infrastructure.ExecutionPolicies;

namespace SFA.DAS.TokenService.Application.UnitTests.PrivilegedAccess.TokenRefresh;

public class HmrcAuthTokenBrokerTests
{
    [Test]
    public async Task GetTokenAsync_InitialRequestFails_ShouldStillGoOnToGetToken()
    {
        var fixtures = new HmrcAuthTokenBrokerTestFixtures()
            .WithInitialTaskResult(() => throw new HttpRequestException("Initial token request has failed", null, HttpStatusCode.InternalServerError));

        var svc = fixtures.CreateHmrcAuthTokenBroker();

        var accessToken = await svc.GetTokenAsync();

        accessToken.Should().NotBeNull();
    }

    [Test]
    public async Task GetTokenAsync_InitialThreeRequestFail_ShouldStillGoOnToGetToken()
    {
        var fixtures = new HmrcAuthTokenBrokerTestFixtures()
            .WithInitialTaskResult(() => throw new HttpRequestException("Initial token request has failed", null, HttpStatusCode.InternalServerError))
            .WithInitialTaskResult(() => throw new HttpRequestException("Initial token request has failed", null, HttpStatusCode.InternalServerError))
            .WithInitialTaskResult(() => null);

        var svc = fixtures.CreateHmrcAuthTokenBroker();

        var accessToken = await svc.GetTokenAsync();

        accessToken.Should().NotBeNull();
    }

    [Test]
    public async Task GetTokenAsync_InitialThreeRequestFail_ShouldCallPostFourTimes()
    {
        var fixtures = new HmrcAuthTokenBrokerTestFixtures()
            .WithInitialTaskResult(() => throw new HttpRequestException("Initial token request has failed", null, HttpStatusCode.InternalServerError))
            .WithInitialTaskResult(() => throw new HttpRequestException("Initial token request has failed", null, HttpStatusCode.InternalServerError))
            .WithInitialTaskResult(() => null);

        var svc = fixtures.CreateHmrcAuthTokenBroker();

        await svc.GetTokenAsync();

        fixtures.OAuthTokenServiceMock.Verify(ots => ots.GetAccessToken(It.IsAny<string>()), Times.Exactly(4));
    }

    [Test]
    public async Task GetTokenAsync_WhenFirstRequestFails_ShouldCallPostTwice()
    {
        var fixtures = new HmrcAuthTokenBrokerTestFixtures()
            .WithInitialTaskResult(() => throw new HttpRequestException("Initial token request has failed", null, HttpStatusCode.InternalServerError));

        var svc = fixtures.CreateHmrcAuthTokenBroker();

        await svc.GetTokenAsync();

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

        token.Should().NotBeNull();
        token!.AccessToken.Should().Be(accessToken, "Access token is not the expected value");
        token.RefreshToken.Should().Be(refreshToken, "Refresh token is not the expected value");
    }
}

public class HmrcAuthTokenBrokerTestFixtures
{
    public HmrcAuthTokenBrokerTestFixtures()
    {
        OAuthTokenServiceMock = new Mock<IOAuthTokenService>();
        SecretRepositoryMock = new Mock<ISecretRepository>();
        _totpServiceMock = new Mock<ITotpService>();
        _tokenRefresherMock = new Mock<ITokenRefresher>();
        _hmrcAuthTokenBrokerConfigMock = new Mock<IHmrcAuthTokenBrokerConfig>();

        _accessTokenRequests = new Queue<Func<OAuthAccessToken?>>();

        _hmrcAuthTokenBrokerConfigMock.Setup(config => config.RetryDelay).Returns(TimeSpan.Zero);

        OAuthTokenServiceMock
            .Setup(ots => ots.GetAccessToken(It.IsAny<string>()))
            .Returns(() =>
            {
                var func = _accessTokenRequests.Dequeue();
                return Task.FromResult(func())!;
            });
    }

    public Mock<IOAuthTokenService> OAuthTokenServiceMock { get; }
    private Mock<ISecretRepository> SecretRepositoryMock { get; }
    private readonly Mock<ITotpService> _totpServiceMock;
    private readonly Mock<ITokenRefresher> _tokenRefresherMock;
    private readonly Mock<IHmrcAuthTokenBrokerConfig> _hmrcAuthTokenBrokerConfigMock;
    private readonly Queue<Func<OAuthAccessToken?>> _accessTokenRequests;

    public HmrcAuthTokenBrokerTestFixtures WithInitialTaskResult(Func<OAuthAccessToken?> getter)
    {
        _accessTokenRequests.Enqueue(getter);
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
            new HmrcExecutionPolicy(Mock.Of<ILogger<HmrcExecutionPolicy>>(), new TimeSpan(0, 0, 0, 0, 10)),
            Mock.Of<ILogger<HmrcAuthTokenBroker>>(),
            OAuthTokenServiceMock.Object,
            SecretRepositoryMock.Object,
            _totpServiceMock.Object,
            _tokenRefresherMock.Object,
            _hmrcAuthTokenBrokerConfigMock.Object
        );
    }
}