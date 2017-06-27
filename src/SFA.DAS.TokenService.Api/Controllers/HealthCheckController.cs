using System.Web.Http;

namespace SFA.DAS.TokenService.Api.Controllers
{
    public class HealthCheckController : ApiController
    {
        [Route("api/HealthCheck")]
        public IHttpActionResult GetStatus()
        {
            return Ok();
        }
    }
}