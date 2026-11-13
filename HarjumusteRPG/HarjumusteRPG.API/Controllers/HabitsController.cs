using System.Security.Claims;
using HarjumusteRPG.API.Dtos;
using HarjumusteRPG.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HarjumusteRPG.API.Controllers;

[ApiController]
[Authorize]
[Route("api/habits")]
[Produces("application/json")]
public class HabitsController : ControllerBase
{
    private readonly IHabitService _habits;

    public HabitsController(IHabitService habits)
    {
        _habits = habits;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Returns the current user's habits with search, filter, sorting and paging.</summary>
    /// <remarks>Example: GET /api/habits?search=water&amp;isActive=true&amp;sortBy=name&amp;page=1&amp;pageSize=10</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<HabitDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<HabitDto>>> GetAll([FromQuery] HabitQueryParams query) =>
        Ok(await _habits.GetPagedAsync(CurrentUserId, query));

    /// <summary>Returns one habit of the current user.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(HabitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HabitDto>> Get(int id)
    {
        var habit = await _habits.GetAsync(id, CurrentUserId);
        return habit is null ? NotFound() : Ok(habit);
    }

    /// <summary>Creates a new habit.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(HabitDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<HabitDto>> Create(CreateHabitDto dto)
    {
        var result = await _habits.CreateAsync(CurrentUserId, dto);
        if (result.Status == ServiceStatus.Conflict)
            return Conflict(new { message = "You already have a habit with this name." });

        return CreatedAtAction(nameof(Get), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>Updates an existing habit.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(HabitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<HabitDto>> Update(int id, UpdateHabitDto dto)
    {
        var result = await _habits.UpdateAsync(id, CurrentUserId, dto);
        return result.Status switch
        {
            ServiceStatus.NotFound => NotFound(),
            ServiceStatus.Conflict => Conflict(new { message = "You already have a habit with this name." }),
            _ => Ok(result.Value)
        };
    }

    /// <summary>Deletes a habit together with its completions.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id) =>
        await _habits.DeleteAsync(id, CurrentUserId) ? NoContent() : NotFound();
}