namespace HarjumusteRPG.API.Dtos;

public class CompletionDto
{
    public int Id { get; set; }
    public int HabitId { get; set; }
    public DateOnly Date { get; set; }
    public int XpEarned { get; set; }
}

public class AchievementDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int RequiredValue { get; set; }
    public bool Unlocked { get; set; }
    public DateTime? UnlockedAt { get; set; }
}

public class CompleteHabitResultDto
{
    public CompletionDto Completion { get; set; } = new();
    public int XpEarned { get; set; }
    public int CoinsEarned { get; set; }
    public bool LeveledUp { get; set; }
    public int Level { get; set; }
    public int Xp { get; set; }
    public int XpForNextLevel { get; set; }
    public int Coins { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public List<AchievementDto> NewAchievements { get; set; } = new();
}

public class ProgressDto
{
    public int Level { get; set; }
    public int Xp { get; set; }
    public int XpForNextLevel { get; set; }
    public int Coins { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public int TotalCompletions { get; set; }
    public List<AchievementDto> Achievements { get; set; } = new();
}