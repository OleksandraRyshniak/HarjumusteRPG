using HarjumusteRPG.API.Dtos;
using HarjumusteRPG.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HarjumusteRPG.API.Controllers;

[ApiController]
[Authorize(Roles = "Service")]
[Route("api/streaks")]
[Produces("application/json")]
public class StreaksController : ControllerBase
{
    private readonly IStreakService _streaks;

    public StreaksController(IStreakService streaks)
    {
        _streaks = streaks;
    }

    /// <summary>Resets streaks of users who missed a day. Called by the Worker (role Service) at 00:05.</summary>
    /// <returns>Users whose streak was reset.</returns>
    [HttpPost("check")]
    [ProducesResponseType(typeof(List<StreakResetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<StreakResetDto>>> Check()
    {
        return Ok(await _streaks.CheckAsync());
    }
}