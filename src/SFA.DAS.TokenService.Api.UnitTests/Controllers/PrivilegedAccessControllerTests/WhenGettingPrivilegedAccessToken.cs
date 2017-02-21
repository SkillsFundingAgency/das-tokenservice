using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Results;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.TokenService.Api.Controllers;
using SFA.DAS.TokenService.Api.Types;
using SFA.DAS.TokenService.Application.PrivilegedAccess.GetPrivilegedAccessToken;

namespace SFA.DAS.TokenService.Api.UnitTests.Controllers.PrivilegedAccessControllerTests
{
    public class WhenGettingPrivilegedAccessToken
    {
        private const string AccessCode = "ACCESS-TOKEN";
        private const int ExpiresIn = 123;
        private Mock<IMediator> _mediator;
        private PrivilegedAccessController _controller;

        [SetUp]
        public void Arrange()
        {
            _mediator = new Mock<IMediator>();
            _mediator.Setup(m => m.SendAsync(It.IsAny<PrivilegedAccessQuery>()))
                .ReturnsAsync(new Domain.OAuthAccessToken
                {
                    AccessToken = AccessCode,
                    ExpiresIn = ExpiresIn
                });

            _controller = new PrivilegedAccessController(_mediator.Object);
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
        public async Task ThenItShouldReturnExpiryTimeAsUtcNowPlusNumberOfSecondsInAccessToken()
        {
            // Act
            var actual = await _controller.GetPrivilegedAccessToken() as OkNegotiatedContentResult<PrivilegedAccessToken>;

            // Assert
            var expectedExpiry = DateTime.UtcNow.AddSeconds(ExpiresIn);
            Assert.IsTrue(actual.Content.ExpiryTime >= expectedExpiry.AddSeconds(-1), 
                $"Expected Expiry time to be greater than or equal to {expectedExpiry.AddSeconds(-1)}, but was {actual.Content.ExpiryTime}");
            Assert.IsTrue(actual.Content.ExpiryTime <= expectedExpiry.AddSeconds(1),
                $"Expected Expiry time to be less than or equal to {expectedExpiry.AddSeconds(1)}, but was {actual.Content.ExpiryTime}");
        }

    }
}
