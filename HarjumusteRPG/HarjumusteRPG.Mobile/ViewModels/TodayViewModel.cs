using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HarjumusteRPG.Mobile.Models;
using HarjumusteRPG.Mobile.Services;
using System.Collections.ObjectModel;
using System.Globalization;

namespace HarjumusteRPG.Mobile.ViewModels;

// One row of the AppResources.NavToday list.
public partial class TodayHabitItem : ObservableObject
{
    public TodayHabitItem(int id, string name, bool done)
    {
        Id = id;
        Name = name;
        IsDone = done;
    }

    public int Id { get; }
    public string Name { get; }
    public string Initial => Name.Length == 0 ? "?" : Name[..1].ToUpperInvariant();

    public string? IconFile { get; init; }
    public bool HasIcon => !string.IsNullOrEmpty(IconFile);
    public bool HasNoIcon => !HasIcon;
    public string Subtitle => AppResources.RewardHint;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CheckText))]
    private bool isDone;

    public string CheckText => IsDone ? "OK" : string.Empty;
}

public partial class TodayViewModel : BaseViewModel
{
    private static CultureInfo Estonian => Loc.Culture;

    private readonly IAuthService _auth;
    private readonly IGameApi _api;

    public TodayViewModel(IAuthService auth, IGameApi api)
    {
        _auth = auth;
        _api = api;
        Title = AppResources.NavToday;
        SectionTitle = Loc.F(AppResources.FeedFormat, PetName);
        PetLine = Loc.F(AppResources.PetWaitingFormat, PetName);
    }

    // The pet name is kept on the device.
    private string PetName => PetStore.Name(_auth.Username);

    public ObservableCollection<TodayHabitItem> Habits { get; } = new();

    [ObservableProperty]
    private string dateText = string.Empty;

    [ObservableProperty]
    private string greeting = string.Empty;

    [ObservableProperty]
    private string petLine = string.Empty;

    [RelayCommand]
    private Task ChangePet() => Shell.Current.GoToAsync("//pet");

    [ObservableProperty]
    private string sectionTitle = string.Empty;

    [ObservableProperty]
    private string levelChip = "LV 1";

    [ObservableProperty]
    private string xpText = "0/100 XP";

    // 0..1 for the XP bar
    [ObservableProperty]
    private double xpFraction;

    [ObservableProperty]
    private string coinsText = "0";

    [ObservableProperty]
    private string streakLine = string.Empty;

    // Message after completing a habit, e.g. "+20 XP  +5 münti"
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasReward))]
    private string? rewardMessage;

    [ObservableProperty]
    private bool isEmpty;

    [ObservableProperty]
    private string petImage = "kass_hall.png";

    public bool HasReward => !string.IsNullOrEmpty(RewardMessage);

    // ---- celebration window: level up / new achievement ----

    [ObservableProperty]
    private bool isCelebrating;

    [ObservableProperty]
    private string celebrationTitle = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasCelebrationLevel))]
    private string celebrationLevel = string.Empty;

    [ObservableProperty]
    private string celebrationReward = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasCelebrationAchievements))]
    private string celebrationAchievements = string.Empty;

    public bool HasCelebrationLevel => !string.IsNullOrEmpty(CelebrationLevel);
    public bool HasCelebrationAchievements => !string.IsNullOrEmpty(CelebrationAchievements);

    // ---- daily gift ----

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(GiftClaimed))]
    private bool giftAvailable;

    public bool GiftClaimed => !GiftAvailable;

    [RelayCommand]
    private async Task ClaimGift()
    {
        if (IsBusy || !GiftAvailable)
            return;

        IsBusy = true;
        try
        {
            ErrorMessage = null;
            RewardMessage = null;

            var result = await _api.ClaimDailyGiftAsync();
            if (!result.Success)
            {
                // claimed on another device -> just hide the button
                if (result.Error == AppResources.ErrGiftClaimed)
                    GiftAvailable = false;

                await HandleErrorAsync(result.Error!);
                return;
            }

            GiftAvailable = false;
            CoinsText = result.Value!.Coins.ToString();
            RewardMessage = Loc.F(AppResources.GiftReceivedFormat, result.Value.CoinsEarned);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ShowCelebration(CompleteResult r)
    {
        CelebrationTitle = r.LeveledUp ? AppResources.LevelUpTitle : AppResources.NewAchievementTitle;
        CelebrationLevel = r.LeveledUp ? Loc.F(AppResources.LevelFormat, r.Level) : string.Empty;
        CelebrationReward = Loc.F(AppResources.RewardFormat, r.XpEarned, r.CoinsEarned);

        var names = r.NewAchievements ?? new List<AchievementInfo>();
        CelebrationAchievements = names.Count == 0
            ? string.Empty
            : (names.Count == 1 ? AppResources.NewAchievementOne : AppResources.NewAchievementMany)
              + Environment.NewLine
              + string.Join(Environment.NewLine, names.Select(a => $"- {AchievementTexts.Name(a)}"));

        IsCelebrating = true;
    }

    [RelayCommand]
    private void CloseCelebration() => IsCelebrating = false;

    [RelayCommand]
    private async Task Load()
    {
        if (IsBusy)
            return;

        DateText = DateTime.Today.ToString("dddd, d. MMM", Estonian).ToUpper(Estonian);
        Greeting = Loc.F(AppResources.GreetingFormat, _auth.Username);
        SectionTitle = Loc.F(AppResources.FeedFormat, PetName);
        PetImage = PetStore.Image(_auth.Username);
        IsBusy = true;
        try
        {
            ErrorMessage = null;

            var progress = await _api.GetProgressAsync();
            if (!progress.Success)
            {
                await HandleErrorAsync(progress.Error!);
                return;
            }
            ShowProgress(progress.Value!.Level, progress.Value.Xp, progress.Value.XpForNextLevel,
                progress.Value.Coins, progress.Value.CurrentStreak);
            GiftAvailable = progress.Value.DailyGiftAvailable;
            var today = await _api.GetTodayAsync();
            if (!today.Success)
            {
                await HandleErrorAsync(today.Error!);
                return;
            }

            Habits.Clear();
            foreach (var h in today.Value!)
                Habits.Add(new TodayHabitItem(h.Id, h.Name, h.Done)
                {
                    IconFile = HabitIcons.GetFile(_auth.Username, h.Id)
                });

            IsEmpty = Habits.Count == 0;
            UpdatePetLine();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task Complete(TodayHabitItem? item)
    {
        if (item is null || item.IsDone || IsBusy)
            return;

        IsBusy = true;
        try
        {
            ErrorMessage = null;
            RewardMessage = null;

            var result = await _api.CompleteAsync(item.Id);
            if (!result.Success)
            {
                await HandleErrorAsync(result.Error!);
                return;
            }

            var r = result.Value!;
            item.IsDone = true;
            ShowProgress(r.Level, r.Xp, r.XpForNextLevel, r.Coins, r.CurrentStreak);
            UpdatePetLine();

            RewardMessage = r.RewardLimitReached
                  ? AppResources.RewardLimit
                  : r.LeveledUp
                  ? Loc.F(AppResources.LevelUpMessageFormat, r.Level, r.XpEarned, r.CoinsEarned)
                : Loc.F(AppResources.RewardFormat, r.XpEarned, r.CoinsEarned);

            if (r.LeveledUp || (r.NewAchievements?.Count ?? 0) > 0)
                ShowCelebration(r);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task AddHabit() => Shell.Current.GoToAsync("habit-edit");

    [RelayCommand]
    private Task OpenSettings() => Shell.Current.GoToAsync("settings");

    [RelayCommand]
    private Task Manage() => Shell.Current.GoToAsync("//habits");

    [RelayCommand]
    private async Task Logout()
    {
        _auth.Logout();
        Habits.Clear();
        await Shell.Current.GoToAsync("//login");
    }

    private void ShowProgress(int level, int xp, int xpForNext, int coins, int streak)
    {
        LevelChip = $"LV {level}";
        XpText = $"{xp}/{xpForNext} XP";
        XpFraction = xpForNext > 0 ? Math.Clamp((double)xp / xpForNext, 0, 1) : 0;
        CoinsText = coins.ToString();
        StreakLine = streak > 0 ? Loc.F(AppResources.StreakFormat, streak) : AppResources.StreakStart;
    }

    // The pet is happy when all of today's habits are done.
    private void UpdatePetLine()
    {
        var done = Habits.Count(h => h.IsDone);
        PetLine = Habits.Count > 0 && done == Habits.Count ? Loc.F(AppResources.PetHappyFormat, PetName)
                : done > 0 ? Loc.F(AppResources.PetGoodFormat, PetName)
                : Loc.F(AppResources.PetWaitingFormat, PetName);
    }

    // Session expired -> back to login, other errors are shown on the screen.
    private async Task HandleErrorAsync(string error)
    {
        if (error.StartsWith(AppResources.ErrSessionExpired))
        {
            _auth.Logout();
            // Load runs while Shell is still finishing the navigation to this page.
            await Task.Delay(300);
            await Shell.Current.GoToAsync("//login");
            return;
        }
        ErrorMessage = error;
    }
}