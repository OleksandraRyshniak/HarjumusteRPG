namespace HarjumusteRPG.API.Models;

public class Friendship
{
    public int Id { get; set; }

    /// <summary>User who sent the invitation.</summary>
    public int UserId { get; set; }

    /// <summary>User who received the invitation.</summary>
    public int FriendId { get; set; }

    public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public User? Friend { get; set; }
}