using HarjumusteRPG.API.Models;

namespace HarjumusteRPG.API.Services;

public static class GameRules
{
    public const int XpPerCompletion = 20;
    public const int CoinsPerCompletion = 5;
    public const int CoinsPerLevelUp = 25;
    public const int FeedCost = 5;
    public const int DailyGiftCoins = 20;

    // Pets in the shop: picture name -> price. Price 0 = free starter pet (cat, dog).
    public static readonly IReadOnlyDictionary<string, int> PetPrices = new Dictionary<string, int>
    {
        ["kass_hall"] = 0,
        ["kutsikas"] = 0,
        ["hamster_hele"] = 50,
        ["hiir"] = 50,
        ["merisiga"] = 80,
        ["kass_pruun"] = 80,
        ["janes_valge"] = 120,
        ["janes_luppkorv"] = 120,
        ["kass_punane"] = 150,
        ["tsintsilja"] = 150
    };

    public const int MaxRewardedPerDay = 5;

    public static int XpForNextLevel(int level) => 100 + (level - 1) * 50;

    public static (int Level, int Xp, int LevelsGained) AddXp(int level, int xp, int amount)
    {
        xp += amount;
        var gained = 0;

        while (xp >= XpForNextLevel(level))
        {
            xp -= XpForNextLevel(level);
            level++;
            gained++;
        }

        return (level, xp, gained);
    }



    // The gift can be claimed once per day.
    public static bool IsGiftAvailable(DateOnly? lastGift, DateOnly today) => lastGift != today;

    public static bool IsScheduledFor(Habit habit, DateOnly date)
    {
        if (habit.IsDaily) return true;
        if (string.IsNullOrEmpty(habit.DaysOfWeek)) return false;

        return habit.DaysOfWeek.Split(',').Select(int.Parse).Contains((int)date.DayOfWeek);
    }

    public static (int Current, int Longest) NextStreak(
      DateOnly? lastCompleted, DateOnly today, int current, int longest, IEnumerable<Habit> habits)
    {
        if (lastCompleted == today) return (current, longest);

        var continues = lastCompleted is not null && !HasMissedDay(habits, lastCompleted.Value, today);
        var next = continues ? current + 1 : 1;
        return (next, Math.Max(longest, next));
    }

    public static bool IsStreakBroken(
        DateOnly? lastCompleted, DateOnly today, int currentStreak, IEnumerable<Habit> habits) =>
        currentStreak > 0 &&
        (lastCompleted is null || HasMissedDay(habits, lastCompleted.Value, today));

    // A day is "missed" only if the user had at least one active habit planned for it.
    // Days with nothing planned (e.g. weekends for Mon-Fri habits) are neutral.
    public static bool HasMissedDay(IEnumerable<Habit> habits, DateOnly lastCompleted, DateOnly today)
    {
        var active = habits.Where(h => h.IsActive).ToList();

        for (var day = lastCompleted.AddDays(1); day < today; day = day.AddDays(1))
        {
            if (active.Any(h => DateOnly.FromDateTime(h.CreatedAt) <= day && IsScheduledFor(h, day)))
                return true;
        }

        return false;
    }

    public static bool IsEarned(Achievement achievement, int level, int streak, int totalCompletions) =>
        achievement.Type switch
        {
            AchievementType.Streak => streak >= achievement.RequiredValue,
            AchievementType.Level => level >= achievement.RequiredValue,
            _ => totalCompletions >= achievement.RequiredValue
        };
}