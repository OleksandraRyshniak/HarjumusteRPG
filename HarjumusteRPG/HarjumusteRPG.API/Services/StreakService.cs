using HarjumusteRPG.API.Dtos;
using HarjumusteRPG.API.Repositories;

namespace HarjumusteRPG.API.Services;

public class StreakService : IStreakService
{
    private readonly IUserRepository _users;
    private readonly TimeProvider _time;
    private readonly ILogger<StreakService> _logger;

    public StreakService(IUserRepository users, TimeProvider time, ILogger<StreakService> logger)
    {
        _users = users;
        _time = time;
        _logger = logger;
    }

    public async Task<List<StreakResetDto>> CheckAsync()
    {
        var today = DateOnly.FromDateTime(_time.GetLocalNow().DateTime);
        var withStreak = await _users.FindAsync(u => u.CurrentStreak > 0);

        var changed = new List<StreakResetDto>();

        foreach (var user in withStreak)
        {
            if (!GameRules.IsStreakBroken(user.LastCompletedDate, today, user.CurrentStreak))
                continue;

            changed.Add(new StreakResetDto
            {
                UserId = user.Id,
                Username = user.Username,
                PreviousStreak = user.CurrentStreak
            });

            user.CurrentStreak = 0;
            _users.Update(user);
        }

        if (changed.Count > 0)
            await _users.SaveChangesAsync();

        _logger.LogInformation("Streak check for {Today}: {Count} streak(s) reset", today, changed.Count);
        return changed;
    }
}