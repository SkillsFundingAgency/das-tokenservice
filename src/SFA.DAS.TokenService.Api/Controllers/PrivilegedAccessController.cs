using System;
using System.Threading.Tasks;
using System.Web.Http;
using SFA.DAS.TokenService.Api.Types;

namespace SFA.DAS.TokenService.Api.Controllers
{
    [RoutePrefix("api/PrivilegedAccess")]
    public class PrivilegedAccessController : ApiController
    {
        [HttpGet]
        [Route("", Name = "GetPrivilegedAccessToken")]
        public async Task<IHttpActionResult> GetPrivilegedAccessToken()
        {
            await Task.Delay(1);

            return Ok(new PrivilegedAccessToken
            {
                AccessCode = "ABC123",
                ExpiryTime = DateTime.Now.AddHours(4)
            });
        }
    }
}
