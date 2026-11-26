namespace HarjumusteRPG.Mobile.Models;

// Shapes of the API answers (only the fields the app needs).
public record HabitInfo(int Id, string Name, bool IsDaily, List<int> DaysOfWeek, bool IsActive);

public record PagedHabits(List<HabitInfo> Items, int Page, int PageSize, int TotalCount);

public record CompletionInfo(int Id, int HabitId, DateOnly Date, int XpEarned);

public record AchievementInfo(
    int Id, string Name, string Description, int RequiredValue, bool Unlocked, DateTime? UnlockedAt);

public record CompleteResult(
    int XpEarned, int CoinsEarned, bool LeveledUp, int Level, int Xp, int XpForNextLevel,
    int Coins, int CurrentStreak, int LongestStreak, List<AchievementInfo> NewAchievements,
    bool RewardLimitReached = false);

public record ProgressInfo(
    int Level, int Xp, int XpForNextLevel, int Coins,
    int CurrentStreak, int LongestStreak, int TotalCompletions, List<AchievementInfo> Achievements,
    bool DailyGiftAvailable, DateOnly Today);

public record DailyGiftResult(int CoinsEarned, int Coins);

public record ShopPet(string Key, int Price, bool Owned);

public record BuyPetResult(string Key, int Price, int Coins);
public record FeedResult(int CoinsSpent, int Coins);
public record DemoTimeResult(DateOnly Today, int DaysAhead, int StreaksReset);