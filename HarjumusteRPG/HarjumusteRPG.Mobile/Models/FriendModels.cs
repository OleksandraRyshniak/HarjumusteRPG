namespace HarjumusteRPG.Mobile.Models;

// Shapes of the friends API answers.
public record LeaderboardEntry(
    int Rank, int UserId, string Username, int Level, int Xp, int CurrentStreak, bool IsMe);

public record FriendInfo(int Id, string Username, int Level, int Xp, int CurrentStreak);

public record FriendRequestInfo(int Id, int UserId, string Username, DateTime CreatedAt);