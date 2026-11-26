using HarjumusteRPG.API.Dtos;
using HarjumusteRPG.API.Models;
using HarjumusteRPG.API.Repositories;
using HarjumusteRPG.API.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace HarjumusteRPG.Tests;

public class CompletionServiceTests
{
    private const int UserId = 5;
    private const int HabitId = 1;
    private static readonly DateOnly Today = new(2026, 11, 23);

    private readonly Mock<IHabitRepository> _habits = new();
    private readonly Mock<IHabitCompletionRepository> _completions = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IAchievementService> _achievements = new();
    private readonly Mock<ILeaderboardNotifier> _notifier = new();

    private CompletionService CreateService() => new(
        _habits.Object,
        _completions.Object,
        _users.Object,
        _achievements.Object,
        _notifier.Object,
        new FixedTimeProvider(new DateTimeOffset(2026, 11, 23, 12, 0, 0, TimeSpan.Zero)),
        NullLogger<CompletionService>.Instance);

    // A daily active habit that is not done yet, and a user on level 1.
    private User ArrangeHabitAndUser(int xp = 0)
    {
        var user = new User { Id = UserId, Level = 1, Xp = xp };

        _habits.Setup(h => h.GetByIdForUserAsync(HabitId, UserId))
            .ReturnsAsync(new Habit { Id = HabitId, UserId = UserId, IsDaily = true, IsActive = true });
        _users.Setup(u => u.GetByIdAsync(UserId)).ReturnsAsync(user);
        _completions.Setup(c => c.ExistsAsync(HabitId, Today)).ReturnsAsync(false);
        _completions.Setup(c => c.CountForUserAsync(UserId)).ReturnsAsync(0);
        _achievements.Setup(a => a.UnlockNewAsync(It.IsAny<User>(), It.IsAny<int>()))
            .ReturnsAsync(new List<AchievementDto>());

        return user;
    }

    [Fact]
    public async Task CompleteAsync_ValidHabit_AddsXpCoinsAndStartsStreak()
    {
        var user = ArrangeHabitAndUser();

        var result = await CreateService().CompleteAsync(HabitId, UserId);

        Assert.Equal(ServiceStatus.Ok, result.Status);
        Assert.Equal(GameRules.XpPerCompletion, user.Xp);
        Assert.Equal(GameRules.CoinsPerCompletion, user.Coins);
        Assert.Equal(1, user.CurrentStreak);
        Assert.Equal(Today, user.LastCompletedDate);
    }

    [Fact]
    public async Task CompleteAsync_ValidHabit_SavesOnceAndNotifiesFriends()
    {
        ArrangeHabitAndUser();

        await CreateService().CompleteAsync(HabitId, UserId);

        _completions.Verify(
            c => c.AddAsync(It.Is<HabitCompletion>(x => x.HabitId == HabitId && x.Date == Today)),
            Times.Once);
        _completions.Verify(c => c.SaveChangesAsync(), Times.Once);
        _notifier.Verify(
            n => n.NotifyAsync(It.Is<int[]>(ids => ids.SequenceEqual(new[] { UserId }))),
            Times.Once);
    }

    [Fact]
    public async Task CompleteAsync_EnoughXp_LevelsUpAndGivesBonusCoins()
    {
        var user = ArrangeHabitAndUser(xp: 90);

        var result = await CreateService().CompleteAsync(HabitId, UserId);

        Assert.True(result.Value!.LeveledUp);
        Assert.Equal(2, user.Level);
        Assert.Equal(GameRules.CoinsPerCompletion + GameRules.CoinsPerLevelUp, user.Coins);
    }

    [Fact]
    public async Task CompleteAsync_AlreadyCompletedToday_ReturnsConflictAndSavesNothing()
    {
        ArrangeHabitAndUser();
        _completions.Setup(c => c.ExistsAsync(HabitId, Today)).ReturnsAsync(true);

        var result = await CreateService().CompleteAsync(HabitId, UserId);

        Assert.Equal(ServiceStatus.Conflict, result.Status);
        _completions.Verify(c => c.SaveChangesAsync(), Times.Never);
        _notifier.Verify(n => n.NotifyAsync(It.IsAny<int[]>()), Times.Never);
    }

    [Fact]
    public async Task CompleteAsync_UnknownHabit_ReturnsNotFound()
    {
        _habits.Setup(h => h.GetByIdForUserAsync(HabitId, UserId)).ReturnsAsync((Habit?)null);
        _users.Setup(u => u.GetByIdAsync(UserId)).ReturnsAsync(new User { Id = UserId });

        var result = await CreateService().CompleteAsync(HabitId, UserId);

        Assert.Equal(ServiceStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task CompleteAsync_DailyLimitReached_SavesCompletionButGivesNoReward()
    {
        var user = ArrangeHabitAndUser();
        user.RewardDate = Today;
        user.RewardsToday = GameRules.MaxRewardedPerDay;

        var result = await CreateService().CompleteAsync(HabitId, UserId);

        Assert.Equal(ServiceStatus.Ok, result.Status);
        Assert.True(result.Value!.RewardLimitReached);
        Assert.Equal(0, result.Value.XpEarned);
        Assert.Equal(0, result.Value.CoinsEarned);
        Assert.Equal(0, user.Xp);
        Assert.Equal(0, user.Coins);
        Assert.Equal(1, user.CurrentStreak);
        _completions.Verify(c => c.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CompleteAsync_NewDay_ResetsRewardCounter()
    {
        var user = ArrangeHabitAndUser();
        user.RewardDate = Today.AddDays(-1);
        user.RewardsToday = GameRules.MaxRewardedPerDay;

        var result = await CreateService().CompleteAsync(HabitId, UserId);

        Assert.False(result.Value!.RewardLimitReached);
        Assert.Equal(GameRules.XpPerCompletion, user.Xp);
        Assert.Equal(1, user.RewardsToday);
        Assert.Equal(Today, user.RewardDate);
    }
}