using System;
using System.Threading.Tasks;
using System.Web.Http;
using MediatR;
using NLog;
using SFA.DAS.TokenService.Api.Types;
using SFA.DAS.TokenService.Application.PrivilegedAccess.GetPrivilegedAccessToken;

namespace SFA.DAS.TokenService.Api.Controllers
{
    [RoutePrefix("api/PrivilegedAccess")]
    public class PrivilegedAccessController : ApiController
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public PrivilegedAccessController(IMediator mediator, ILogger logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        [Route("", Name = "GetPrivilegedAccessToken")]
        [Authorize]
        public async Task<IHttpActionResult> GetPrivilegedAccessToken()
        {
            try
            {
                var accessToken = await _mediator.SendAsync(new PrivilegedAccessQuery());

                return Ok(new PrivilegedAccessToken
                {
                    AccessCode = accessToken.AccessToken,
                    ExpiryTime = accessToken.ExpiresAt
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting privileged access token - " + ex.Message);
                return InternalServerError();
            }
        }
    }
}
