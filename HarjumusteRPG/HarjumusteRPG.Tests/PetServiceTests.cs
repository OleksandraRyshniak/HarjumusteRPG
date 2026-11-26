using System.Linq.Expressions;
using HarjumusteRPG.API.Models;
using HarjumusteRPG.API.Repositories;
using HarjumusteRPG.API.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace HarjumusteRPG.Tests;

public class PetServiceTests
{
    private const int UserId = 5;

    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IRepository<UserPet>> _pets = new();

    private PetService CreateService() => new(_users.Object, _pets.Object, NullLogger<PetService>.Instance);

    private User ArrangeUser(int coins, params string[] ownedPets)
    {
        var user = new User { Id = UserId, Coins = coins };
        _users.Setup(u => u.GetByIdAsync(UserId)).ReturnsAsync(user);

        var owned = ownedPets.Select(k => new UserPet { UserId = UserId, PetKey = k }).ToList();
        _pets.Setup(p => p.FindAsync(It.IsAny<Expression<Func<UserPet, bool>>>()))
            .ReturnsAsync((Expression<Func<UserPet, bool>> filter) => owned.Where(filter.Compile()).ToList());
        return user;
    }

    [Fact]
    public async Task BuyAsync_EnoughCoins_TakesCoinsAndSavesPet()
    {
        var user = ArrangeUser(coins: 70);

        var result = await CreateService().BuyAsync(UserId, "hamster_hele");

        Assert.Equal(ServiceStatus.Ok, result.Status);
        Assert.Equal(70 - GameRules.PetPrices["hamster_hele"], user.Coins);
        _pets.Verify(p => p.AddAsync(It.Is<UserPet>(x => x.PetKey == "hamster_hele" && x.UserId == UserId)), Times.Once);
        _pets.Verify(p => p.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task BuyAsync_NotEnoughCoins_ReturnsInvalidAndKeepsCoins()
    {
        var user = ArrangeUser(coins: 10);

        var result = await CreateService().BuyAsync(UserId, "tsintsilja");

        Assert.Equal(ServiceStatus.Invalid, result.Status);
        Assert.Equal(10, user.Coins);
        _pets.Verify(p => p.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task BuyAsync_AlreadyOwned_ReturnsConflict()
    {
        ArrangeUser(coins: 500, "hiir");

        var result = await CreateService().BuyAsync(UserId, "hiir");

        Assert.Equal(ServiceStatus.Conflict, result.Status);
    }

    [Fact]
    public async Task BuyAsync_UnknownPet_ReturnsNotFound()
    {
        ArrangeUser(coins: 500);

        var result = await CreateService().BuyAsync(UserId, "dinosaurus");

        Assert.Equal(ServiceStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task GetShopAsync_StarterAndBoughtPetsAreOwned()
    {
        ArrangeUser(coins: 0, "merisiga");

        var result = await CreateService().GetShopAsync(UserId);

        var list = result.Value!;
        Assert.True(list.Single(p => p.Key == "kass_hall").Owned);
        Assert.True(list.Single(p => p.Key == "merisiga").Owned);
        Assert.False(list.Single(p => p.Key == "tsintsilja").Owned);
    }

    [Fact]
    public async Task FeedAsync_EnoughCoins_TakesCoins()
    {
        var user = ArrangeUser(coins: 12);

        var result = await CreateService().FeedAsync(UserId);

        Assert.Equal(ServiceStatus.Ok, result.Status);
        Assert.Equal(12 - GameRules.FeedCost, user.Coins);
        _users.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task FeedAsync_NotEnoughCoins_ReturnsInvalidAndKeepsCoins()
    {
        var user = ArrangeUser(coins: GameRules.FeedCost - 1);

        var result = await CreateService().FeedAsync(UserId);

        Assert.Equal(ServiceStatus.Invalid, result.Status);
        Assert.Equal(GameRules.FeedCost - 1, user.Coins);
        _users.Verify(u => u.SaveChangesAsync(), Times.Never);
    }
}