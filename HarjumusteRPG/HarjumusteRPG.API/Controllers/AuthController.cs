using HarjumusteRPG.API.Dtos;
using HarjumusteRPG.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace HarjumusteRPG.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    /// <summary>Registers a new user with the User role.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDto>> Register(RegisterDto dto)
    {
        var user = await _auth.RegisterAsync(dto);
        if (user is null)
        {
            return Conflict(new { message = "Email or username is already in use." });
        }

        return Created($"/api/users/{user.Id}", user);
    }

    /// <summary>Logs in with email and password.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDto>> Login(LoginDto dto)
    {
        var user = await _auth.LoginAsync(dto);
        if (user is null)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        return Ok(user);
    }
}