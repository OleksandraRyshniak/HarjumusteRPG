namespace HarjumusteRPG.API.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public int Xp { get; set; }
    public int Level { get; set; } = 1;
    public int Coins { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public DateOnly? LastCompletedDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Habit> Habits { get; set; } = new();
    public List<UserAchievement> UserAchievements { get; set; } = new();
}