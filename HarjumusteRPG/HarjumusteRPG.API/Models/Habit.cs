namespace HarjumusteRPG.API.Models;

public class Habit
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsDaily { get; set; } = true;
    public string? DaysOfWeek { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public List<HabitCompletion> Completions { get; set; } = new();
}