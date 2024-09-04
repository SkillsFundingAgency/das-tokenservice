using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.TokenService.Api.Controllers;

[Route("api/HealthCheck")]
public class HealthCheckController : Controller
{
    [HttpGet]
    [Route("")]
    public IActionResult GetStatus()
    {
        return Ok();
    }
}