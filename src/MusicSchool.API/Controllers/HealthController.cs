using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace MusicSchool.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/health")]
public sealed class HealthController(IStringLocalizer<SharedResource> localizer) : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "Healthy",
            message = localizer["HealthMessage"].Value,
            timestampUtc = DateTimeOffset.UtcNow
        });
    }
}
