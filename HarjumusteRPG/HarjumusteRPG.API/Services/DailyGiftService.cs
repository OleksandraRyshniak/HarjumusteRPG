using HarjumusteRPG.API.Dtos;
using HarjumusteRPG.API.Repositories;

namespace HarjumusteRPG.API.Services;

public class DailyGiftService : IDailyGiftService
{
    private readonly IUserRepository _users;
    private readonly TimeProvider _time;
    private readonly ILogger<DailyGiftService> _logger;

    public DailyGiftService(IUserRepository users, TimeProvider time, ILogger<DailyGiftService> logger)
    {
        _users = users;
        _time = time;
        _logger = logger;
    }

    public async Task<ServiceResult<DailyGiftDto>> ClaimAsync(int userId)
    {
        var user = await _users.GetByIdAsync(userId);
        if (user is null)
            return new ServiceResult<DailyGiftDto>(ServiceStatus.NotFound);

        var today = DateOnly.FromDateTime(_time.GetLocalNow().DateTime);
        if (!GameRules.IsGiftAvailable(user.LastGiftDate, today))
            return new ServiceResult<DailyGiftDto>(ServiceStatus.Conflict, null, "The daily gift is already claimed today.");

        user.Coins += GameRules.DailyGiftCoins;
        user.LastGiftDate = today;
        await _users.SaveChangesAsync();

        _logger.LogInformation("User {UserId} claimed the daily gift, coins now {Coins}", userId, user.Coins);

        return new ServiceResult<DailyGiftDto>(ServiceStatus.Ok, new DailyGiftDto
        {
            CoinsEarned = GameRules.DailyGiftCoins,
            Coins = user.Coins
        });
    }
}