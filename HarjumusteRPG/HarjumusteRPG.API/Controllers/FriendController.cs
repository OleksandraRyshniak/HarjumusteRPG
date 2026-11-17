using System.Security.Claims;
using HarjumusteRPG.API.Dtos;
using HarjumusteRPG.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HarjumusteRPG.API.Controllers;

[ApiController]
[Authorize]
[Route("api/friends")]
[Produces("application/json")]
public class FriendsController : ControllerBase
{
    private readonly IFriendService _friends;

    public FriendsController(IFriendService friends)
    {
        _friends = friends;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private ObjectResult ErrorFor(ServiceStatus status, string? error) =>
        Problem(detail: error, statusCode: status switch
        {
            ServiceStatus.NotFound => StatusCodes.Status404NotFound,
            ServiceStatus.Invalid => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status409Conflict
        });

    /// <summary>Returns the current user's friends.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<FriendDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<FriendDto>>> GetFriends() =>
        Ok(await _friends.GetFriendsAsync(CurrentUserId));

    /// <summary>Returns incoming friend requests waiting for a reply.</summary>
    [HttpGet("requests")]
    [ProducesResponseType(typeof(List<FriendRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<FriendRequestDto>>> GetRequests() =>
        Ok(await _friends.GetIncomingRequestsAsync(CurrentUserId));

    /// <summary>Sends a friend request to a user by username.</summary>
    [HttpPost("requests")]
    [ProducesResponseType(typeof(FriendRequestDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FriendRequestDto>> SendRequest(SendFriendRequestDto dto)
    {
        var result = await _friends.SendRequestAsync(CurrentUserId, dto.Username);
        return result.Status == ServiceStatus.Ok
            ? CreatedAtAction(nameof(GetRequests), result.Value)
            : ErrorFor(result.Status, result.Error);
    }

    /// <summary>Accepts an incoming friend request.</summary>
    [HttpPost("requests/{id:int}/accept")]
    [ProducesResponseType(typeof(FriendDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FriendDto>> Accept(int id)
    {
        var result = await _friends.AcceptAsync(id, CurrentUserId);
        return result.Status == ServiceStatus.Ok ? Ok(result.Value) : ErrorFor(result.Status, result.Error);
    }

    /// <summary>Declines an incoming request or cancels an outgoing one.</summary>
    [HttpDelete("requests/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Decline(int id)
    {
        var result = await _friends.DeclineAsync(id, CurrentUserId);
        return result.Status == ServiceStatus.Ok ? NoContent() : ErrorFor(result.Status, result.Error);
    }

    /// <summary>Removes a friend by user id.</summary>
    [HttpDelete("{friendId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(int friendId)
    {
        var result = await _friends.RemoveFriendAsync(CurrentUserId, friendId);
        return result.Status == ServiceStatus.Ok ? NoContent() : ErrorFor(result.Status, result.Error);
    }
}