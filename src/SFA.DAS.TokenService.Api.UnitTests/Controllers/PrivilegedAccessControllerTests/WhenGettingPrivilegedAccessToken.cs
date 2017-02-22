using System;
using System.Threading.Tasks;
using System.Web.Http.Results;
using MediatR;
using Moq;
using NLog;
using NUnit.Framework;
using SFA.DAS.TokenService.Api.Controllers;
using SFA.DAS.TokenService.Api.Types;
using SFA.DAS.TokenService.Application.PrivilegedAccess.GetPrivilegedAccessToken;

namespace SFA.DAS.TokenService.Api.UnitTests.Controllers.PrivilegedAccessControllerTests
{
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
            _mediator.Setup(m => m.SendAsync(It.IsAny<PrivilegedAccessQuery>()))
                .ReturnsAsync(new Domain.OAuthAccessToken
                {
                    AccessToken = AccessCode,
                    ExpiresAt = ExpiresAt
                });

            _logger = new Mock<ILogger>();

            _controller = new PrivilegedAccessController(_mediator.Object, _logger.Object);
        }

        [Test]
        public async Task ThenItShouldReturnAnOkResult()
        {
            // Act
            var actual = await _controller.GetPrivilegedAccessToken();

            // Assert
            Assert.IsNotNull(actual);
            Assert.IsInstanceOf<OkNegotiatedContentResult<PrivilegedAccessToken>>(actual);
        }

        [Test]
        public async Task ThenItShouldReturnAccessCode()
        {
            // Act
            var actual = await _controller.GetPrivilegedAccessToken() as OkNegotiatedContentResult<PrivilegedAccessToken>;

            // Assert
            Assert.AreEqual(AccessCode, actual.Content.AccessCode);
        }

        [Test]
        public async Task ThenItShouldReturnExpiryTime()
        {
            // Act
            var actual = await _controller.GetPrivilegedAccessToken() as OkNegotiatedContentResult<PrivilegedAccessToken>;

            // Assert
            Assert.AreEqual(ExpiresAt, actual.Content.ExpiryTime);
        }

        [Test]
        public async Task ThenItShouldReturnInternalServerErrorWhenExceptionOccurs()
        {
            // Arrange
            _mediator.Setup(m => m.SendAsync(It.IsAny<PrivilegedAccessQuery>()))
                .ThrowsAsync(new Exception("Unit test"));

            // Act
            var actual = await _controller.GetPrivilegedAccessToken();

            // Assert
            Assert.IsNotNull(actual);
            Assert.IsInstanceOf<InternalServerErrorResult>(actual);
        }

    }
}
