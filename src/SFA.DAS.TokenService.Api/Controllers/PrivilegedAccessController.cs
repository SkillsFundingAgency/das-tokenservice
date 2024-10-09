using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.TokenService.Api.Types;
using SFA.DAS.TokenService.Application.PrivilegedAccess.GetPrivilegedAccessToken;

namespace SFA.DAS.TokenService.Api.Controllers;

[Authorize]
[Route("api/PrivilegedAccess")]
public class PrivilegedAccessController(IMediator mediator, ILogger<PrivilegedAccessController> logger) : Controller
{
    [HttpGet]
    [Route("", Name = "GetPrivilegedAccessToken")]
    public async Task<IActionResult> GetPrivilegedAccessToken()
    {
        try
        {
            var accessToken = await mediator.Send(new PrivilegedAccessQuery());

            return Ok(new PrivilegedAccessToken
            {
                AccessCode = accessToken.AccessToken,
                ExpiryTime = accessToken.ExpiresAt
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting privileged access token.");
            return StatusCode(500);
        }
    }
}