using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.TokenService.Api.Controllers;

[AllowAnonymous]
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