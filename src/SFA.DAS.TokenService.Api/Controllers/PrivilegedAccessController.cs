using System;
using System.Threading.Tasks;
using System.Web.Http;
using MediatR;
using SFA.DAS.TokenService.Api.Types;
using SFA.DAS.TokenService.Application.PrivilegedAccess.GetPrivilegedAccessToken;

namespace SFA.DAS.TokenService.Api.Controllers
{
    [RoutePrefix("api/PrivilegedAccess")]
    public class PrivilegedAccessController : ApiController
    {
        private readonly IMediator _mediator;

        public PrivilegedAccessController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route("", Name = "GetPrivilegedAccessToken")]
        public async Task<IHttpActionResult> GetPrivilegedAccessToken()
        {
            var accessToken = await _mediator.SendAsync(new PrivilegedAccessQuery());

            return Ok(new PrivilegedAccessToken
            {
                AccessCode = accessToken.AccessToken,
                ExpiryTime = DateTime.UtcNow.AddSeconds(accessToken.ExpiresIn)
            });
        }
    }
}
