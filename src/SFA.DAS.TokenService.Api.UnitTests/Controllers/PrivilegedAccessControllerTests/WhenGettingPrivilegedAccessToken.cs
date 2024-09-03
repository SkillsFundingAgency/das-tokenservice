using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.TokenService.Api.Controllers;
using SFA.DAS.TokenService.Api.Types;
using SFA.DAS.TokenService.Application.PrivilegedAccess.GetPrivilegedAccessToken;
using SFA.DAS.TokenService.Domain;

namespace SFA.DAS.TokenService.Api.UnitTests.Controllers.PrivilegedAccessControllerTests;

public class WhenGettingPrivilegedAccessToken
{
    private const string AccessCode = "ACCESS-TOKEN";
    private readonly DateTime ExpiresAt = new DateTime(2017, 2, 22, 13, 45, 26);

    private Mock<IMediator> _mediator;
    private PrivilegedAccessController _controller;
    private Mock<ILogger> _logger;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
        _mediator.Setup(m => m.Send(It.IsAny<PrivilegedAccessQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OAuthAccessToken
            {
                AccessToken = AccessCode,
                ExpiresAt = ExpiresAt
            });

        _logger = new Mock<ILogger>();

        _controller = new PrivilegedAccessController(_mediator.Object, _logger.Object);
    }

    [TearDown]
    public void TearDown() => _controller.Dispose();

    [Test]
    public async Task ThenItShouldReturnAnOkResult()
    {
        // Act
        var actual = await _controller.GetPrivilegedAccessToken() as OkObjectResult;

        // Assert
        actual.Should().NotBeNull();
        var model = actual.Value as PrivilegedAccessToken;
        model.Should().NotBeNull();
    }

    [Test]
    public async Task ThenItShouldReturnAccessCode()
    {
        // Act
        var actual = await _controller.GetPrivilegedAccessToken() as OkObjectResult;

        // Assert
        var model = actual.Value as PrivilegedAccessToken;
        model.AccessCode.Should().Be(AccessCode);
    }

    [Test]
    public async Task ThenItShouldReturnExpiryTime()
    {
        // Act
        var actual = await _controller.GetPrivilegedAccessToken() as OkObjectResult;

        // Assert
        var model = actual.Value as PrivilegedAccessToken;
        model.ExpiryTime.Should().Be(ExpiresAt);
    }

    [Test]
    public async Task ThenItShouldReturnInternalServerErrorWhenExceptionOccurs()
    {
        // Arrange
        _mediator.Setup(m => m.Send(It.IsAny<PrivilegedAccessQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unit test"));

        // Act
        var actual = await _controller.GetPrivilegedAccessToken() as StatusCodeResult;

        // Assert
        actual.Should().NotBeNull();
        actual.StatusCode.Should().Be(500);
    }
}