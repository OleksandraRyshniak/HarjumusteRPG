using HarjumusteRPG.API.Models;
using HarjumusteRPG.API.Repositories;
using HarjumusteRPG.API.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace HarjumusteRPG.Tests;

public class DailyGiftServiceTests
{
    private const int UserId = 5;
    private static readonly DateOnly Today = new(2026, 11, 23);

    private readonly Mock<IUserRepository> _users = new();

    private DailyGiftService CreateService() => new(
        _users.Object,
        new FixedTimeProvider(new DateTimeOffset(2026, 11, 23, 12, 0, 0, TimeSpan.Zero)),
        NullLogger<DailyGiftService>.Instance);

    private User ArrangeUser(DateOnly? lastGift, int coins = 10)
    {
        var user = new User { Id = UserId, Coins = coins, LastGiftDate = lastGift };
        _users.Setup(u => u.GetByIdAsync(UserId)).ReturnsAsync(user);
        return user;
    }

    [Fact]
    public async Task ClaimAsync_FirstClaimToday_AddsCoinsAndSavesDate()
    {
        var user = ArrangeUser(lastGift: null);

        var result = await CreateService().ClaimAsync(UserId);

        Assert.Equal(ServiceStatus.Ok, result.Status);
        Assert.Equal(10 + GameRules.DailyGiftCoins, user.Coins);
        Assert.Equal(Today, user.LastGiftDate);
        Assert.Equal(user.Coins, result.Value!.Coins);
        _users.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ClaimAsync_AlreadyClaimedToday_ReturnsConflictAndKeepsCoins()
    {
        var user = ArrangeUser(lastGift: Today);

        var result = await CreateService().ClaimAsync(UserId);

        Assert.Equal(ServiceStatus.Conflict, result.Status);
        Assert.Equal(10, user.Coins);
        _users.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task ClaimAsync_ClaimedYesterday_AllowsNewGift()
    {
        var user = ArrangeUser(lastGift: Today.AddDays(-1));

        var result = await CreateService().ClaimAsync(UserId);

        Assert.Equal(ServiceStatus.Ok, result.Status);
        Assert.Equal(Today, user.LastGiftDate);
    }

    [Fact]
    public async Task ClaimAsync_UnknownUser_ReturnsNotFound()
    {
        _users.Setup(u => u.GetByIdAsync(UserId)).ReturnsAsync((User?)null);

        var result = await CreateService().ClaimAsync(UserId);

        Assert.Equal(ServiceStatus.NotFound, result.Status);
    }
}