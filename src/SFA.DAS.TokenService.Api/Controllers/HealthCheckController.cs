using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.TokenService.Api.Controllers;

public class HealthCheckController : Controller
{
    [Route("api/HealthCheck")]
    public IActionResult GetStatus()
    {
        return Ok();
    }
}