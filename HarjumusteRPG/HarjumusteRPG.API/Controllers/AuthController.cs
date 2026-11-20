using System.Security.Claims;
using HarjumusteRPG.API.Dtos;
using HarjumusteRPG.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HarjumusteRPG.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService auth, ILogger<AuthController> logger)
    {
        _auth = auth;
        _logger = logger;
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
            _logger.LogWarning("Registration rejected, email or username already in use: {Email}", dto.Email);
            return Problem(detail: "Email or username is already in use.", statusCode: StatusCodes.Status409Conflict);
        }

        _logger.LogInformation("User registered: {UserId}", user.Id);
        return Created($"/api/users/{user.Id}", user);
    }

    /// <summary>Logs in with email and password and returns a JWT token.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var result = await _auth.LoginAsync(dto);
        if (result is null)
        {
            _logger.LogWarning("Failed login attempt for {Email}", dto.Email);
            return Problem(detail: "Invalid email or password.", statusCode: StatusCodes.Status401Unauthorized);
        }

        _logger.LogInformation("User logged in: {UserId}", result.User.Id);
        return Ok(result);
    }

    /// <summary>Returns the current user from the JWT token.</summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Me() => Ok(new
    {
        id = User.FindFirstValue(ClaimTypes.NameIdentifier),
        username = User.Identity?.Name,
        role = User.FindFirstValue(ClaimTypes.Role)
    });

    /// <summary>Test endpoint available only to the Admin role.</summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("admin-check")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult AdminCheck() => Ok(new { message = "You are Admin." });
}