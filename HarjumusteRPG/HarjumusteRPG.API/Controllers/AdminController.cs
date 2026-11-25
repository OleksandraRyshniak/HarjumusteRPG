using System.Security.Claims;
using HarjumusteRPG.API.Dtos;
using HarjumusteRPG.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HarjumusteRPG.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin")]
[Produces("application/json")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _admin;

    public AdminController(IAdminService admin)
    {
        _admin = admin;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Returns all users with search, role filter and paging.</summary>
    /// <remarks>Example: GET /api/admin/users?search=ali&amp;role=User&amp;page=1&amp;pageSize=10</remarks>
    [HttpGet("users")]
    [ProducesResponseType(typeof(PagedResult<AdminUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<AdminUserDto>>> GetUsers([FromQuery] AdminUserQueryParams query) =>
        Ok(await _admin.GetUsersAsync(query));

    /// <summary>Deletes a user together with habits, completions and friendships.</summary>
    [HttpDelete("users/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var result = await _admin.DeleteUserAsync(id, CurrentUserId);
        return result.Status switch
        {
            ServiceStatus.NotFound => NotFound(),
            ServiceStatus.Invalid => Problem(detail: result.Error, statusCode: StatusCodes.Status400BadRequest),
            _ => NoContent()
        };
    }

    /// <summary>Returns all achievements.</summary>
    [HttpGet("achievements")]
    [ProducesResponseType(typeof(List<AdminAchievementDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<AdminAchievementDto>>> GetAchievements() =>
        Ok(await _admin.GetAchievementsAsync());

    /// <summary>Creates a new achievement.</summary>
    [HttpPost("achievements")]
    [ProducesResponseType(typeof(AdminAchievementDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AdminAchievementDto>> CreateAchievement(AchievementEditDto dto)
    {
        var result = await _admin.CreateAchievementAsync(dto);
        if (result.Status == ServiceStatus.Conflict)
            return Problem(detail: "An achievement with this name already exists.", statusCode: StatusCodes.Status409Conflict);

        return Created($"/api/admin/achievements/{result.Value!.Id}", result.Value);
    }

    /// <summary>Updates an achievement.</summary>
    [HttpPut("achievements/{id:int}")]
    [ProducesResponseType(typeof(AdminAchievementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AdminAchievementDto>> UpdateAchievement(int id, AchievementEditDto dto)
    {
        var result = await _admin.UpdateAchievementAsync(id, dto);
        return result.Status switch
        {
            ServiceStatus.NotFound => NotFound(),
            ServiceStatus.Conflict => Problem(detail: "An achievement with this name already exists.", statusCode: StatusCodes.Status409Conflict),
            _ => Ok(result.Value)
        };
    }

    /// <summary>Deletes an achievement. Users lose it as well.</summary>
    [HttpDelete("achievements/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAchievement(int id) =>
        (await _admin.DeleteAchievementAsync(id)).Status == ServiceStatus.NotFound ? NotFound() : NoContent();
}