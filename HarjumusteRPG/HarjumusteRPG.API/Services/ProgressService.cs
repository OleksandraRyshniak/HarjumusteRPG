using HarjumusteRPG.API.Dtos;
using HarjumusteRPG.API.Repositories;

namespace HarjumusteRPG.API.Services;

public class ProgressService : IProgressService
{
    private readonly IUserRepository _users;
    private readonly IHabitCompletionRepository _completions;
    private readonly IAchievementService _achievements;
    private readonly TimeProvider _time;

    public ProgressService(
        IUserRepository users,
        IHabitCompletionRepository completions,
        IAchievementService achievements,
        TimeProvider time)
    {
        _users = users;
        _completions = completions;
        _achievements = achievements;
        _time = time;
    }

    public async Task<ProgressDto?> GetAsync(int userId)
    {
        var user = await _users.GetByIdAsync(userId);
        if (user is null) return null;

        var today = DateOnly.FromDateTime(_time.GetLocalNow().DateTime);

        return new ProgressDto
        {
            Level = user.Level,
            Xp = user.Xp,
            XpForNextLevel = GameRules.XpForNextLevel(user.Level),
            Coins = user.Coins,
            CurrentStreak = user.CurrentStreak,
            LongestStreak = user.LongestStreak,
            TotalCompletions = await _completions.CountForUserAsync(userId),
            DailyGiftAvailable = GameRules.IsGiftAvailable(user.LastGiftDate, today),
            Today = today,
            Achievements = await _achievements.GetForUserAsync(userId)
        };
    }
}