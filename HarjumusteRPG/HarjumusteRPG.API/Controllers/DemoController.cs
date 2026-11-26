using HarjumusteRPG.API.Dtos;
using HarjumusteRPG.API.Infrastructure;
using HarjumusteRPG.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HarjumusteRPG.API.Controllers;

[ApiController]
[Authorize]
[Route("api/demo")]
[Produces("application/json")]
public class DemoController : ControllerBase
{
    private readonly DemoTimeProvider _time;
    private readonly IStreakService _streaks;
    private readonly IWebHostEnvironment _env;

    public DemoController(DemoTimeProvider time, IStreakService streaks, IWebHostEnvironment env)
    {
        _time = time;
        _streaks = streaks;
        _env = env;
    }

    /// <summary>Development only: moves the API clock one day forward and runs the streak check (like the Worker at 00:05).</summary>
    [HttpPost("next-day")]
    [ProducesResponseType(typeof(DemoTimeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DemoTimeDto>> NextDay()
    {
        if (!_env.IsDevelopment())
            return NotFound();

        _time.AddDays(1);
        var reset = await _streaks.CheckAsync();
        return Ok(Current(reset.Count));
    }

    /// <summary>Development only: puts the API clock back to the real time.</summary>
    [HttpPost("reset")]
    [ProducesResponseType(typeof(DemoTimeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<DemoTimeDto> Reset()
    {
        if (!_env.IsDevelopment())
            return NotFound();

        _time.Reset();
        return Ok(Current(0));
    }

    private DemoTimeDto Current(int streaksReset) => new()
    {
        Today = DateOnly.FromDateTime(_time.GetLocalNow().DateTime),
        DaysAhead = (int)_time.Offset.TotalDays,
        StreaksReset = streaksReset
    };
}