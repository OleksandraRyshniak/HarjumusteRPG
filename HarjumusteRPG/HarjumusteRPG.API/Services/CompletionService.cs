using HarjumusteRPG.API.Dtos;
using HarjumusteRPG.API.Models;
using HarjumusteRPG.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HarjumusteRPG.API.Services;

public class CompletionService : ICompletionService
{
    private readonly IHabitRepository _habits;
    private readonly IHabitCompletionRepository _completions;
    private readonly IUserRepository _users;
    private readonly IAchievementService _achievements;
    private readonly TimeProvider _time;
    private readonly ILogger<CompletionService> _logger;
    private readonly ILeaderboardNotifier _notifier;

    public CompletionService(
     IHabitRepository habits,
     IHabitCompletionRepository completions,
     IUserRepository users,
     IAchievementService achievements,
     ILeaderboardNotifier notifier,
     TimeProvider time,
     ILogger<CompletionService> logger)
    {
        _habits = habits;
        _completions = completions;
        _users = users;
        _achievements = achievements;
        _notifier = notifier;
        _time = time;
        _logger = logger;
    }

    public async Task<ServiceResult<CompleteHabitResultDto>> CompleteAsync(int habitId, int userId)
    {
        var habit = await _habits.GetByIdForUserAsync(habitId, userId);
        var user = await _users.GetByIdAsync(userId);
        if (habit is null || user is null)
            return new ServiceResult<CompleteHabitResultDto>(ServiceStatus.NotFound);

        var today = DateOnly.FromDateTime(_time.GetLocalNow().DateTime);

        if (!habit.IsActive)
            return new ServiceResult<CompleteHabitResultDto>(ServiceStatus.Invalid, null, "This habit is not active.");

        if (!GameRules.IsScheduledFor(habit, today))
            return new ServiceResult<CompleteHabitResultDto>(ServiceStatus.Invalid, null, "This habit is not scheduled for today.");

        if (await _completions.ExistsAsync(habitId, today))
            return new ServiceResult<CompleteHabitResultDto>(ServiceStatus.Conflict, null, "This habit is already completed today.");

        // new day: the daily reward counter starts again
        if (user.RewardDate != today)
        {
            user.RewardDate = today;
            user.RewardsToday = 0;
        }

        var rewarded = user.RewardsToday < GameRules.MaxRewardedPerDay;
        var xpEarned = rewarded ? GameRules.XpPerCompletion : 0;
        var coinsEarned = 0;
        var gained = 0;

        var completion = new HabitCompletion
        {
            HabitId = habit.Id,
            Date = today,
            XpEarned = xpEarned
        };
        await _completions.AddAsync(completion);

        if (rewarded)
        {
            var (level, xp, levelsGained) = GameRules.AddXp(user.Level, user.Xp, xpEarned);
            gained = levelsGained;
            coinsEarned = GameRules.CoinsPerCompletion + gained * GameRules.CoinsPerLevelUp;

            user.Level = level;
            user.Xp = xp;
            user.Coins += coinsEarned;
            user.RewardsToday++;
        }

        // the streak and the history count even when the reward limit is reached
        (user.CurrentStreak, user.LongestStreak) =
            GameRules.NextStreak(user.LastCompletedDate, today, user.CurrentStreak, user.LongestStreak);
        user.LastCompletedDate = today;

        // +1 because the new completion is not saved yet
        var total = await _completions.CountForUserAsync(userId) + 1;
        var newAchievements = await _achievements.UnlockNewAsync(user, total);

        try
        {
            // All repositories share one DbContext, so this saves the completion,
            // the user changes and the achievements in a single transaction.
            await _completions.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // unique index (HabitId, Date): two simultaneous requests
            return new ServiceResult<CompleteHabitResultDto>(ServiceStatus.Conflict, null, "This habit is already completed today.");
        }
        _logger.LogInformation(
           "User {UserId} completed habit {HabitId}: rewarded {Rewarded}, level {Level}, streak {Streak}, new achievements {Count}",
           userId, habitId, rewarded, user.Level, user.CurrentStreak, newAchievements.Count);

        await _notifier.NotifyAsync(userId);

        return new ServiceResult<CompleteHabitResultDto>(ServiceStatus.Ok, new CompleteHabitResultDto
        {
            Completion = ToDto(completion),
            XpEarned = xpEarned,
            CoinsEarned = coinsEarned,
            RewardLimitReached = !rewarded,
            LeveledUp = gained > 0,
            Level = user.Level,
            Xp = user.Xp,
            XpForNextLevel = GameRules.XpForNextLevel(user.Level),
            Coins = user.Coins,
            CurrentStreak = user.CurrentStreak,
            LongestStreak = user.LongestStreak,
            NewAchievements = newAchievements
        });
    }
       
    

    public async Task<List<CompletionDto>?> GetHistoryAsync(int habitId, int userId, int days)
    {
        var habit = await _habits.GetByIdForUserAsync(habitId, userId);
        if (habit is null) return null;

        var items = await _completions.GetByHabitAsync(habitId, days);
        return items.Select(ToDto).ToList();
    }

    private static CompletionDto ToDto(HabitCompletion c) => new()
    {
        Id = c.Id,
        HabitId = c.HabitId,
        Date = c.Date,
        XpEarned = c.XpEarned
    };
}