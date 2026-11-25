namespace HarjumusteRPG.Mobile.Models;

// Body of POST /api/habits and PUT /api/habits/{id}.
public record HabitRequest(string Name, bool IsDaily, List<int> DaysOfWeek, bool IsActive);

public static class Weekdays
{
    // Monday first; values are the same as in the API (0 = Sunday ... 6 = Saturday).
    public static readonly int[] Order = { 1, 2, 3, 4, 5, 6, 0 };

    // Letters of Sunday ... Saturday in the selected language (P,E,T,K,N,R,L / S,M,T,W,T,F,S).
    public static string Letter(int day) => AppResources.WeekdayLetters.Split(',')[day];
}