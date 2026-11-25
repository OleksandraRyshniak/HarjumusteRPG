namespace HarjumusteRPG.API.Models;

public class Achievement
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public AchievementType Type { get; set; }
    public int RequiredValue { get; set; }

    public List<UserAchievement> UserAchievements { get; set; } = new();
}


