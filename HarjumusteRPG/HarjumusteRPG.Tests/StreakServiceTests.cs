using System.Linq.Expressions;
using HarjumusteRPG.API.Models;
using HarjumusteRPG.API.Repositories;
using HarjumusteRPG.API.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace HarjumusteRPG.Tests;

public class StreakServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<ILeaderboardNotifier> _notifier = new();

    // "now" is 23 Nov 2026 00:05, the time when the Worker runs
    private StreakService CreateService() => new(
        _users.Object,
        _notifier.Object,
        new FixedTimeProvider(new DateTimeOffset(2026, 11, 23, 0, 5, 0, TimeSpan.Zero)),
        NullLogger<StreakService>.Instance);

    private void ArrangeUsersWithStreak(params User[] users) =>
        _users.Setup(u => u.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(users.ToList());

    [Fact]
    public async Task CheckAsync_MissedDay_ResetsStreakAndNotifies()
    {
        var user = new User
        {
            Id = 3,
            Username = "sasha",
            CurrentStreak = 5,
            LastCompletedDate = new DateOnly(2026, 11, 20)
        };
        ArrangeUsersWithStreak(user);

        var reset = await CreateService().CheckAsync();

        var item = Assert.Single(reset);
        Assert.Equal(5, item.PreviousStreak);
        Assert.Equal(0, user.CurrentStreak);
        _users.Verify(u => u.SaveChangesAsync(), Times.Once);
        _notifier.Verify(
            n => n.NotifyAsync(It.Is<int[]>(ids => ids.SequenceEqual(new[] { 3 }))),
            Times.Once);
    }

    [Fact]
    public async Task CheckAsync_CompletedYesterday_KeepsStreakAndSavesNothing()
    {
        var user = new User
        {
            Id = 3,
            Username = "sasha",
            CurrentStreak = 4,
            LastCompletedDate = new DateOnly(2026, 11, 22)
        };
        ArrangeUsersWithStreak(user);

        var reset = await CreateService().CheckAsync();

        Assert.Empty(reset);
        Assert.Equal(4, user.CurrentStreak);
        _users.Verify(u => u.SaveChangesAsync(), Times.Never);
        _notifier.Verify(n => n.NotifyAsync(It.IsAny<int[]>()), Times.Never);
    }
}