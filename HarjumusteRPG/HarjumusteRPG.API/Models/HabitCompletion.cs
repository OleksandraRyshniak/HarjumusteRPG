namespace HarjumusteRPG.API.Models;

public class HabitCompletion
{
    public int Id { get; set; }
    public int HabitId { get; set; }
    public DateOnly Date { get; set; }
    public int XpEarned { get; set; }

    public Habit? Habit { get; set; }
}