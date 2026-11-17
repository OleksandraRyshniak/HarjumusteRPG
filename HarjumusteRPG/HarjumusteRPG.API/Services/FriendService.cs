using HarjumusteRPG.API.Dtos;
using HarjumusteRPG.API.Models;
using HarjumusteRPG.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HarjumusteRPG.API.Services;

public class FriendService : IFriendService
{
    private readonly IFriendshipRepository _friendships;
    private readonly IUserRepository _users;
    private readonly ILogger<FriendService> _logger;

    public FriendService(IFriendshipRepository friendships, IUserRepository users, ILogger<FriendService> logger)
    {
        _friendships = friendships;
        _users = users;
        _logger = logger;
    }

    public async Task<List<FriendDto>> GetFriendsAsync(int userId)
    {
        var accepted = await _friendships.GetAcceptedForUserAsync(userId);
        return accepted
            .Select(f => ToFriendDto(f.UserId == userId ? f.Friend! : f.User!))
            .OrderBy(f => f.Username)
            .ToList();
    }

    public async Task<List<FriendRequestDto>> GetIncomingRequestsAsync(int userId)
    {
        var incoming = await _friendships.GetIncomingAsync(userId);
        return incoming.Select(f => new FriendRequestDto
        {
            Id = f.Id,
            UserId = f.UserId,
            Username = f.User!.Username,
            CreatedAt = f.CreatedAt
        }).ToList();
    }

    public async Task<List<LeaderboardEntryDto>> GetLeaderboardAsync(int userId)
    {
        var accepted = await _friendships.GetAcceptedForUserAsync(userId);
        var users = accepted.Select(f => f.UserId == userId ? f.Friend! : f.User!).ToList();

        var me = await _users.GetByIdAsync(userId);
        if (me is not null) users.Add(me);

        return users
            .OrderByDescending(u => u.Level)
            .ThenByDescending(u => u.Xp)
            .ThenByDescending(u => u.CurrentStreak)
            .ThenBy(u => u.Username)
            .Select((u, index) => new LeaderboardEntryDto
            {
                Rank = index + 1,
                UserId = u.Id,
                Username = u.Username,
                Level = u.Level,
                Xp = u.Xp,
                CurrentStreak = u.CurrentStreak,
                IsMe = u.Id == userId
            })
            .ToList();
    }

    public async Task<ServiceResult<FriendRequestDto>> SendRequestAsync(int userId, string username)
    {
        var target = await _users.GetByUsernameAsync(username);
        if (target is null)
            return new ServiceResult<FriendRequestDto>(ServiceStatus.NotFound, null, "User not found.");

        if (target.Id == userId)
            return new ServiceResult<FriendRequestDto>(ServiceStatus.Invalid, null, "You cannot add yourself as a friend.");

        var existing = await _friendships.GetBetweenAsync(userId, target.Id);
        if (existing is not null)
        {
            var message = existing.Status == FriendshipStatus.Accepted
                ? "You are already friends."
                : "A friend request between you already exists.";
            return new ServiceResult<FriendRequestDto>(ServiceStatus.Conflict, null, message);
        }

        var friendship = new Friendship
        {
            UserId = userId,
            FriendId = target.Id,
            Status = FriendshipStatus.Pending
        };
        await _friendships.AddAsync(friendship);

        try
        {
            await _friendships.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return new ServiceResult<FriendRequestDto>(ServiceStatus.Conflict, null, "A friend request between you already exists.");
        }

        _logger.LogInformation("User {UserId} sent a friend request to user {TargetId}", userId, target.Id);

        return new ServiceResult<FriendRequestDto>(ServiceStatus.Ok, new FriendRequestDto
        {
            Id = friendship.Id,
            UserId = target.Id,
            Username = target.Username,
            CreatedAt = friendship.CreatedAt
        });
    }

    public async Task<ServiceResult<FriendDto>> AcceptAsync(int requestId, int userId)
    {
        var friendship = await _friendships.GetWithUsersAsync(requestId);

        // Only the recipient can accept a pending request.
        if (friendship is null || friendship.FriendId != userId || friendship.Status != FriendshipStatus.Pending)
            return new ServiceResult<FriendDto>(ServiceStatus.NotFound, null, "Friend request not found.");

        friendship.Status = FriendshipStatus.Accepted;
        _friendships.Update(friendship);
        await _friendships.SaveChangesAsync();

        _logger.LogInformation("User {UserId} accepted friend request {RequestId}", userId, requestId);

        return new ServiceResult<FriendDto>(ServiceStatus.Ok, ToFriendDto(friendship.User!));
    }

    public async Task<ServiceResult<bool>> DeclineAsync(int requestId, int userId)
    {
        var friendship = await _friendships.GetByIdAsync(requestId);

        // Both sides can cancel/decline a pending request.
        if (friendship is null || friendship.Status != FriendshipStatus.Pending ||
            (friendship.UserId != userId && friendship.FriendId != userId))
            return new ServiceResult<bool>(ServiceStatus.NotFound, false, "Friend request not found.");

        _friendships.Remove(friendship);
        await _friendships.SaveChangesAsync();
        return new ServiceResult<bool>(ServiceStatus.Ok, true);
    }

    public async Task<ServiceResult<bool>> RemoveFriendAsync(int userId, int friendId)
    {
        var friendship = await _friendships.GetBetweenAsync(userId, friendId);

        if (friendship is null || friendship.Status != FriendshipStatus.Accepted)
            return new ServiceResult<bool>(ServiceStatus.NotFound, false, "Friend not found.");

        _friendships.Remove(friendship);
        await _friendships.SaveChangesAsync();

        _logger.LogInformation("User {UserId} removed friend {FriendId}", userId, friendId);
        return new ServiceResult<bool>(ServiceStatus.Ok, true);
    }

    private static FriendDto ToFriendDto(User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Level = user.Level,
        Xp = user.Xp,
        CurrentStreak = user.CurrentStreak
    };
}