namespace HarjumusteRPG.Mobile.Models;

// Shapes of the API answers (only the fields the app needs).
public record HabitInfo(int Id, string Name, bool IsDaily, List<int> DaysOfWeek, bool IsActive);

public record PagedHabits(List<HabitInfo> Items, int Page, int PageSize, int TotalCount);

public record CompletionInfo(int Id, int HabitId, DateOnly Date, int XpEarned);

public record CompleteResult(
    int XpEarned, int CoinsEarned, bool LeveledUp, int Level, int Xp, int XpForNextLevel,
    int Coins, int CurrentStreak, int LongestStreak);

public record ProgressInfo(
    int Level, int Xp, int XpForNextLevel, int Coins,
    int CurrentStreak, int LongestStreak, int TotalCompletions);