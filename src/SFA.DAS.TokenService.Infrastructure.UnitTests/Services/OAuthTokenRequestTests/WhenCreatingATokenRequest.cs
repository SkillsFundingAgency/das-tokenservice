using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.TokenService.Infrastructure.Services;

namespace SFA.DAS.TokenService.Infrastructure.UnitTests.Services.OAuthTokenRequestTests;

public class WhenCreatingATokenRequest
{
    [Test]
    public void ThenItShouldPopulateTokenRequestProperties()
    {
        var clientId = Guid.NewGuid().ToString();
        var clientSecret = Guid.NewGuid().ToString();

        var request = OAuthTokenRequest.Create(clientId, clientSecret);

        request.ClientId.Should().Be(clientId);
        request.ClientSecret.Should().Be(clientSecret);
        request.RefreshToken.Should().BeNullOrEmpty();
        request.Scopes.Should().Be("read:apprenticeship-levy");
        request.GrantType.Should().Be("client_credentials");
    }

    [Test]
    public void ThenItShouldPopulateTokenRequestPropertiesForARefreshTokenRequest()
    {
        var clientId = Guid.NewGuid().ToString();
        var clientSecret = Guid.NewGuid().ToString();
        var refreshToken = Guid.NewGuid().ToString();

        var request = OAuthTokenRequest.Create(clientId, clientSecret, refreshToken);

        request.ClientId.Should().Be(clientId);
        request.ClientSecret.Should().Be(clientSecret);
        request.RefreshToken.Should().Be(refreshToken);
        request.Scopes.Should().Be("read:apprenticeship-levy");
        request.GrantType.Should().Be("client_credentials");
    }
}