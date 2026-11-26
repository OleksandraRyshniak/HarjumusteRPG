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
    private readonly IPetService _pets;

    public MeController(IProgressService progress, IDailyGiftService dailyGift, IPetService pets)
    {
        _progress = progress;
        _dailyGift = dailyGift;
        _pets = pets;
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

    /// <summary>Shop: all pets with their price and whether the user already has them.</summary>
    [HttpGet("pets")]
    [ProducesResponseType(typeof(List<ShopPetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ShopPetDto>>> GetPets()
    {
        var result = await _pets.GetShopAsync(CurrentUserId);
        return result.Status == ServiceStatus.NotFound ? NotFound() : Ok(result.Value);
    }

    /// <summary>Buys a pet with coins. 400 = not enough coins, 409 = the pet is already owned.</summary>
    /// <param name="key">Pet key, e.g. "hamster_hele".</param>
    [HttpPost("pets/{key}/buy")]
    [ProducesResponseType(typeof(BuyPetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BuyPetDto>> BuyPet(string key)
    {
        var result = await _pets.BuyAsync(CurrentUserId, key);
        return result.Status switch
        {
            ServiceStatus.NotFound => NotFound(),
            ServiceStatus.Invalid => Problem(detail: result.Error, statusCode: StatusCodes.Status400BadRequest),
            ServiceStatus.Conflict => Problem(detail: result.Error, statusCode: StatusCodes.Status409Conflict),
            _ => Ok(result.Value)
        };
    }

    /// <summary>Feeds the pet. Costs 5 coins; 400 if the user does not have enough coins.</summary>
    [HttpPost("pet/feed")]
    [ProducesResponseType(typeof(FeedDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FeedDto>> FeedPet()
    {
        var result = await _pets.FeedAsync(CurrentUserId);
        return result.Status switch
        {
            ServiceStatus.NotFound => NotFound(),
            ServiceStatus.Invalid => Problem(detail: result.Error, statusCode: StatusCodes.Status400BadRequest),
            _ => Ok(result.Value)
        };
    }
}