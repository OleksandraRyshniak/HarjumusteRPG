using HarjumusteRPG.API.Models;
using HarjumusteRPG.API.Services;
using Xunit;

namespace HarjumusteRPG.Tests;

public class GameRulesTests
{
    private static readonly DateOnly Today = new(2026, 11, 23); // Monday

    // a daily habit: every day is planned
    private static readonly Habit[] Daily =
    {
        new Habit { IsDaily = true, IsActive = true, CreatedAt = new DateTime(2026, 1, 1) }
    };

    // Monday-Friday only: Saturday and Sunday are not planned
    private static readonly Habit[] Weekdays =
    {
        new Habit { IsDaily = false, DaysOfWeek = "1,2,3,4,5", IsActive = true, CreatedAt = new DateTime(2026, 1, 1) }
    };

    [Theory]
    [InlineData(1, 100)]
    [InlineData(2, 150)]
    [InlineData(3, 200)]
    public void XpForNextLevel_GrowsBy50PerLevel(int level, int expected)
    {
        Assert.Equal(expected, GameRules.XpForNextLevel(level));
    }

    [Fact]
    public void AddXp_NotEnoughXp_NoLevelUp()
    {
        var (level, xp, gained) = GameRules.AddXp(1, 0, 20);

        Assert.Equal(1, level);
        Assert.Equal(20, xp);
        Assert.Equal(0, gained);
    }

    [Fact]
    public void AddXp_EnoughXp_LevelsUpAndKeepsRemainder()
    {
        var (level, xp, gained) = GameRules.AddXp(1, 90, 20);

        Assert.Equal(2, level);
        Assert.Equal(10, xp);
        Assert.Equal(1, gained);
    }

    [Fact]
    public void AddXp_HugeAmount_LevelsUpSeveralTimes()
    {
        // 300 - 100 = 200 -> level 2; 200 - 150 = 50 -> level 3; 50 < 200
        var (level, xp, gained) = GameRules.AddXp(1, 0, 300);

        Assert.Equal(3, level);
        Assert.Equal(50, xp);
        Assert.Equal(2, gained);
    }

    [Fact]
    public void NextStreak_CompletedYesterday_ExtendsStreak()
    {
        var (current, longest) = GameRules.NextStreak(Today.AddDays(-1), Today, 3, 3, Daily);

        Assert.Equal(4, current);
        Assert.Equal(4, longest);
    }

    [Fact]
    public void NextStreak_MissedDay_StartsOverAndKeepsLongest()
    {
        var (current, longest) = GameRules.NextStreak(Today.AddDays(-3), Today, 5, 7, Daily);

        Assert.Equal(1, current);
        Assert.Equal(7, longest);
    }

    [Fact]
    public void NextStreak_WeekendWithoutPlannedHabits_ContinuesStreak()
    {
        // last completion on Friday, today is Monday, nothing planned on Sat/Sun
        var (current, longest) = GameRules.NextStreak(Today.AddDays(-3), Today, 5, 7, Weekdays);

        Assert.Equal(6, current);
        Assert.Equal(7, longest);
    }

    [Fact]
    public void NextStreak_FirstEverCompletion_StartsStreakOfOne()
    {
        var (current, longest) = GameRules.NextStreak(null, Today, 0, 0, Daily);

        Assert.Equal(1, current);
        Assert.Equal(1, longest);
    }

    [Fact]
    public void NextStreak_AlreadyCompletedToday_ChangesNothing()
    {
        var (current, longest) = GameRules.NextStreak(Today, Today, 4, 6, Daily);

        Assert.Equal(4, current);
        Assert.Equal(6, longest);
    }

    [Fact]
    public void IsScheduledFor_DailyHabit_IsAlwaysTrue()
    {
        var habit = new Habit { IsDaily = true };

        Assert.True(GameRules.IsScheduledFor(habit, Today));
    }

    [Theory]
    [InlineData("1,3", true)]   // Monday and Wednesday
    [InlineData("2,4", false)]  // Tuesday and Thursday
    public void IsScheduledFor_SelectedWeekdays_MatchesDayOfWeek(string days, bool expected)
    {
        var habit = new Habit { IsDaily = false, DaysOfWeek = days };

        Assert.Equal(expected, GameRules.IsScheduledFor(habit, Today));
    }

    [Theory]
    [InlineData(1, 5, false)] // completed yesterday: the streak is alive
    [InlineData(2, 5, true)]  // yesterday was missed
    [InlineData(2, 0, false)] // there is no streak to break
    public void IsStreakBroken_DependsOnLastCompletedDate(int daysAgo, int streak, bool expected)
    {
        Assert.Equal(expected, GameRules.IsStreakBroken(Today.AddDays(-daysAgo), Today, streak, Daily));
    }

    [Fact]
    public void IsStreakBroken_WeekendWithoutPlannedHabits_IsNotBroken()
    {
        // last completion on Friday, Sat/Sun had nothing planned
        Assert.False(GameRules.IsStreakBroken(Today.AddDays(-3), Today, 5, Weekdays));
    }

    [Fact]
    public void IsEarned_ChecksTheRightValueForEachType()
    {
        var streakAchievement = new Achievement { Type = AchievementType.Streak, RequiredValue = 7 };
        var levelAchievement = new Achievement { Type = AchievementType.Level, RequiredValue = 5 };
        var totalAchievement = new Achievement { Type = AchievementType.TotalCompletions, RequiredValue = 100 };

        Assert.True(GameRules.IsEarned(streakAchievement, level: 1, streak: 7, totalCompletions: 0));
        Assert.False(GameRules.IsEarned(levelAchievement, level: 4, streak: 99, totalCompletions: 999));
        Assert.True(GameRules.IsEarned(totalAchievement, level: 1, streak: 0, totalCompletions: 100));
    }
}