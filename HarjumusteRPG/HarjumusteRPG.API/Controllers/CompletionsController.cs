using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using HarjumusteRPG.API.Dtos;
using HarjumusteRPG.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HarjumusteRPG.API.Controllers;

[ApiController]
[Authorize]
[Route("api/habits/{habitId:int}/completions")]
[Produces("application/json")]
public class CompletionsController : ControllerBase
{
    private readonly ICompletionService _completions;

    public CompletionsController(ICompletionService completions)
    {
        _completions = completions;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Marks the habit as done today. The API calculates XP, level, coins, streak and achievements.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CompleteHabitResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CompleteHabitResultDto>> Complete(int habitId)
    {
        var result = await _completions.CompleteAsync(habitId, CurrentUserId);
        return result.Status switch
        {
            ServiceStatus.NotFound => NotFound(),
            ServiceStatus.Invalid => BadRequest(new { message = result.Error }),
            ServiceStatus.Conflict => Conflict(new { message = result.Error }),
            _ => CreatedAtAction(nameof(GetHistory), new { habitId }, result.Value)
        };
    }

    /// <summary>Returns the completion history of a habit for the last N days.</summary>
    /// <param name="habitId">Habit id.</param>
    /// <param name="days">How many days back (1 to 365, default 30).</param>
    [HttpGet]
    [ProducesResponseType(typeof(List<CompletionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<CompletionDto>>> GetHistory(int habitId, [FromQuery][Range(1, 365)] int days = 30)
    {
        var history = await _completions.GetHistoryAsync(habitId, CurrentUserId, days);
        return history is null ? NotFound() : Ok(history);
    }
}