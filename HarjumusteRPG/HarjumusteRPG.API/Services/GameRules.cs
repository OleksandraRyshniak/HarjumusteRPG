using HarjumusteRPG.API.Models;

namespace HarjumusteRPG.API.Services;

public static class GameRules
{
    public const int XpPerCompletion = 20;
    public const int CoinsPerCompletion = 5;
    public const int CoinsPerLevelUp = 25;

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

    public static (int Current, int Longest) NextStreak(DateOnly? lastCompleted, DateOnly today, int current, int longest)
    {
        if (lastCompleted == today) return (current, longest);

        var next = lastCompleted == today.AddDays(-1) ? current + 1 : 1;
        return (next, Math.Max(longest, next));
    }

    public static bool IsScheduledFor(Habit habit, DateOnly date)
    {
        if (habit.IsDaily) return true;
        if (string.IsNullOrEmpty(habit.DaysOfWeek)) return false;

        return habit.DaysOfWeek.Split(',').Select(int.Parse).Contains((int)date.DayOfWeek);
    }

    public static bool IsEarned(Achievement achievement, int level, int streak, int totalCompletions) =>
        achievement.Type switch
        {
            AchievementType.Streak => streak >= achievement.RequiredValue,
            AchievementType.Level => level >= achievement.RequiredValue,
            _ => totalCompletions >= achievement.RequiredValue
        };
}