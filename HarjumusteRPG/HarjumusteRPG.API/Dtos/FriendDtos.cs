using System.ComponentModel.DataAnnotations;

namespace HarjumusteRPG.API.Dtos;

public class SendFriendRequestDto
{
    [Required, StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;
}

public class FriendDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public int Level { get; set; }
    public int Xp { get; set; }
    public int CurrentStreak { get; set; }
}

public class FriendRequestDto
{
    public int Id { get; set; }

    /// <summary>The other user: sender of an incoming request or recipient of an outgoing one.</summary>
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class LeaderboardEntryDto
{
    public int Rank { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; }
    public int Level { get; set; }
    public int Xp { get; set; }
    public int CurrentStreak { get; set; }
    public bool IsMe { get; set; }
}