using HarjumusteRPG.API.Models;
using HarjumusteRPG.API.Repositories;
using HarjumusteRPG.API.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace HarjumusteRPG.Tests;

public class FriendServiceTests
{
    private readonly Mock<IFriendshipRepository> _friendships = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<ILeaderboardNotifier> _notifier = new();

    private FriendService CreateService() => new(
        _friendships.Object, _users.Object, _notifier.Object, NullLogger<FriendService>.Instance);

    [Fact]
    public async Task SendRequestAsync_UnknownUsername_ReturnsNotFound()
    {
        _users.Setup(u => u.GetByUsernameAsync("nobody")).ReturnsAsync((User?)null);

        var result = await CreateService().SendRequestAsync(1, "nobody");

        Assert.Equal(ServiceStatus.NotFound, result.Status);
        _friendships.Verify(f => f.AddAsync(It.IsAny<Friendship>()), Times.Never);
    }

    [Fact]
    public async Task SendRequestAsync_Yourself_ReturnsInvalid()
    {
        _users.Setup(u => u.GetByUsernameAsync("alexa")).ReturnsAsync(new User { Id = 1, Username = "alexa" });

        var result = await CreateService().SendRequestAsync(1, "alexa");

        Assert.Equal(ServiceStatus.Invalid, result.Status);
        _friendships.Verify(f => f.AddAsync(It.IsAny<Friendship>()), Times.Never);
    }

    [Fact]
    public async Task SendRequestAsync_RequestAlreadyExists_ReturnsConflict()
    {
        _users.Setup(u => u.GetByUsernameAsync("sasha")).ReturnsAsync(new User { Id = 2, Username = "sasha" });
        _friendships.Setup(f => f.GetBetweenAsync(1, 2))
            .ReturnsAsync(new Friendship { UserId = 1, FriendId = 2, Status = FriendshipStatus.Pending });

        var result = await CreateService().SendRequestAsync(1, "sasha");

        Assert.Equal(ServiceStatus.Conflict, result.Status);
    }

    [Fact]
    public async Task SendRequestAsync_NewFriend_CreatesPendingRequest()
    {
        Friendship? added = null;
        _users.Setup(u => u.GetByUsernameAsync("sasha")).ReturnsAsync(new User { Id = 2, Username = "sasha" });
        _friendships.Setup(f => f.GetBetweenAsync(1, 2)).ReturnsAsync((Friendship?)null);
        _friendships.Setup(f => f.AddAsync(It.IsAny<Friendship>()))
            .Callback<Friendship>(f => added = f)
            .Returns(Task.CompletedTask);

        var result = await CreateService().SendRequestAsync(1, "sasha");

        Assert.Equal(ServiceStatus.Ok, result.Status);
        Assert.Equal("sasha", result.Value!.Username);
        Assert.NotNull(added);
        Assert.Equal(1, added!.UserId);
        Assert.Equal(2, added.FriendId);
        Assert.Equal(FriendshipStatus.Pending, added.Status);
        _friendships.Verify(f => f.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AcceptAsync_ByRecipient_AcceptsAndNotifiesBothUsers()
    {
        var friendship = new Friendship
        {
            Id = 10,
            UserId = 1,
            FriendId = 2,
            Status = FriendshipStatus.Pending,
            User = new User { Id = 1, Username = "alexa" }
        };
        _friendships.Setup(f => f.GetWithUsersAsync(10)).ReturnsAsync(friendship);

        var result = await CreateService().AcceptAsync(10, 2);

        Assert.Equal(ServiceStatus.Ok, result.Status);
        Assert.Equal(FriendshipStatus.Accepted, friendship.Status);
        _notifier.Verify(
            n => n.NotifyAsync(It.Is<int[]>(ids => ids.Order().SequenceEqual(new[] { 1, 2 }))),
            Times.Once);
    }

    [Fact]
    public async Task AcceptAsync_BySender_ReturnsNotFoundAndSavesNothing()
    {
        var friendship = new Friendship
        {
            Id = 10,
            UserId = 1,
            FriendId = 2,
            Status = FriendshipStatus.Pending,
            User = new User { Id = 1, Username = "alexa" }
        };
        _friendships.Setup(f => f.GetWithUsersAsync(10)).ReturnsAsync(friendship);

        // user 1 sent the request, so only user 2 may accept it
        var result = await CreateService().AcceptAsync(10, 1);

        Assert.Equal(ServiceStatus.NotFound, result.Status);
        _friendships.Verify(f => f.SaveChangesAsync(), Times.Never);
    }
}