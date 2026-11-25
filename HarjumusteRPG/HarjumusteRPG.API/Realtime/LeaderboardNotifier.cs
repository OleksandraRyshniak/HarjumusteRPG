using System.Text.Json;
using HarjumusteRPG.API.Realtime;
using HarjumusteRPG.API.Repositories;

namespace HarjumusteRPG.API.Services;

public interface ILeaderboardNotifier
{
    /// <summary>Tells the friends of the given users (and the users themselves) that the leaderboard changed.</summary>
    Task NotifyAsync(params int[] changedUserIds);
}

public class LeaderboardNotifier : ILeaderboardNotifier
{
    private readonly IFriendshipRepository _friendships;
    private readonly IConnectionRegistry _registry;
    private readonly ILogger<LeaderboardNotifier> _logger;

    public LeaderboardNotifier(
        IFriendshipRepository friendships, IConnectionRegistry registry, ILogger<LeaderboardNotifier> logger)
    {
        _friendships = friendships;
        _registry = registry;
        _logger = logger;
    }

    public async Task NotifyAsync(params int[] changedUserIds)
    {
        try
        {
            foreach (var userId in changedUserIds.Distinct())
            {
                var accepted = await _friendships.GetAcceptedForUserAsync(userId);
                var recipients = accepted
                    .Select(f => f.UserId == userId ? f.FriendId : f.UserId)
                    .Append(userId)
                    .Distinct();

                // channel friends-of-{userId}: the friends of the user and the user
                var message = JsonSerializer.Serialize(new
                {
                    type = "LeaderboardChanged",
                    channel = $"friends-of-{userId}",
                    userId
                });

                foreach (var recipient in recipients)
                    await _registry.SendAsync(recipient, message);
            }
        }
        catch (Exception ex)
        {
            // a failed notification must not break the request that has already saved the data
            _logger.LogWarning(ex, "Leaderboard notification failed");
        }
    }
}