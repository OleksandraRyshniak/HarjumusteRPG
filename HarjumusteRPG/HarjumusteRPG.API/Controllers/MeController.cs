using System.Security.Claims;
using HarjumusteRPG.API.Dtos;
using HarjumusteRPG.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HarjumusteRPG.API.Controllers;

[ApiController]
[Authorize]
[Route("api/me")]
[Produces("application/json")]
public class MeController : ControllerBase
{
    private readonly IProgressService _progress;

    public MeController(IProgressService progress)
    {
        _progress = progress;
    }

    /// <summary>Returns the current user's level, XP, coins, streak and achievements.</summary>
    [HttpGet("progress")]
    [ProducesResponseType(typeof(ProgressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProgressDto>> GetProgress()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var progress = await _progress.GetAsync(userId);
        return progress is null ? NotFound() : Ok(progress);
    }
}