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
    private readonly IDailyGiftService _dailyGift;

    public MeController(IProgressService progress, IDailyGiftService dailyGift)
    {
        _progress = progress;
        _dailyGift = dailyGift;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Returns the current user's level, XP, coins, streak, achievements and whether the daily gift can be claimed.</summary>
    [HttpGet("progress")]
    [ProducesResponseType(typeof(ProgressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProgressDto>> GetProgress()
    {
        var progress = await _progress.GetAsync(CurrentUserId);
        return progress is null ? NotFound() : Ok(progress);
    }

    /// <summary>Claims the daily gift (20 coins). It can be claimed once per day.</summary>
    [HttpPost("daily-gift")]
    [ProducesResponseType(typeof(DailyGiftDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DailyGiftDto>> ClaimDailyGift()
    {
        var result = await _dailyGift.ClaimAsync(CurrentUserId);
        return result.Status switch
        {
            ServiceStatus.NotFound => NotFound(),
            ServiceStatus.Conflict => Problem(detail: result.Error, statusCode: StatusCodes.Status409Conflict),
            _ => Ok(result.Value)
        };
    }
}