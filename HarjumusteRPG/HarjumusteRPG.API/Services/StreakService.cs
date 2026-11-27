using HarjumusteRPG.API.Dtos;
using HarjumusteRPG.API.Repositories;

namespace HarjumusteRPG.API.Services;

public class StreakService : IStreakService
{
    private readonly IUserRepository _users;
    private readonly IHabitRepository _habits;
    private readonly TimeProvider _time;
    private readonly ILogger<StreakService> _logger;
    private readonly ILeaderboardNotifier _notifier;

    public StreakService(
        IUserRepository users, IHabitRepository habits, ILeaderboardNotifier notifier,
        TimeProvider time, ILogger<StreakService> logger)
    {
        _users = users;
        _habits = habits;
        _notifier = notifier;
        _time = time;
        _logger = logger;
    }

    public async Task<List<StreakResetDto>> CheckAsync()
    {
        var today = DateOnly.FromDateTime(_time.GetLocalNow().DateTime);
        var withStreak = await _users.FindAsync(u => u.CurrentStreak > 0);

        // active habits of these users, loaded with one query
        var userIds = withStreak.Select(u => u.Id).ToList();
        var habitsByUser = (await _habits.FindAsync(h => userIds.Contains(h.UserId) && h.IsActive))
            .ToLookup(h => h.UserId);

        var changed = new List<StreakResetDto>();

        foreach (var user in withStreak)
        {
            if (!GameRules.IsStreakBroken(user.LastCompletedDate, today, user.CurrentStreak, habitsByUser[user.Id]))
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
        {
            await _users.SaveChangesAsync();
            await _notifier.NotifyAsync(changed.Select(c => c.UserId).ToArray());
        }

        _logger.LogInformation("Streak check for {Today}: {Count} streak(s) reset", today, changed.Count);
        return changed;
    }
}