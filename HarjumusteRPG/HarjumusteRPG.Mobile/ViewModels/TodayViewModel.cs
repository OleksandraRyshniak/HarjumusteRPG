using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HarjumusteRPG.Mobile.Services;
using System.Collections.ObjectModel;
using System.Globalization;

namespace HarjumusteRPG.Mobile.ViewModels;

// One row of the "Täna" list.
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
    public string Subtitle => "+20 XP · +5 münti";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CheckText))]
    private bool isDone;

    public string CheckText => IsDone ? "OK" : string.Empty;
}

public partial class TodayViewModel : BaseViewModel
{
    private static readonly CultureInfo Estonian = new("et-EE");

    private readonly IAuthService _auth;
    private readonly IGameApi _api;

    public TodayViewModel(IAuthService auth, IGameApi api)
    {
        _auth = auth;
        _api = api;
        Title = "Täna";
        SectionTitle = $"Toida {PetName} harjumustega";
        PetLine = $"{PetName} ootab sind!";
    }

    // The pet name is kept on the device (a full pet screen comes later).
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

    public bool HasReward => !string.IsNullOrEmpty(RewardMessage);

    [RelayCommand]
    private async Task Load()
    {
        if (IsBusy)
            return;

        DateText = DateTime.Today.ToString("dddd, d. MMM", Estonian).ToUpper(Estonian);
        Greeting = $"Tere, {_auth.Username}!";
        SectionTitle = $"Toida {PetName} harjumustega";
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

            RewardMessage = r.LeveledUp
                ? $"TASE TÕUSIS! Nüüd tase {r.Level}  +{r.XpEarned} XP  +{r.CoinsEarned} münti"
                : $"+{r.XpEarned} XP  +{r.CoinsEarned} münti";
        }
        finally
        {
            IsBusy = false;
        }
    }


    [RelayCommand]
    private Task AddHabit() => Shell.Current.GoToAsync("habit-edit");

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
        StreakLine = streak > 0 ? $"{streak} päeva seeria" : "Alusta seeriat täna!";
    }

    [ObservableProperty]
    private string petImage = "kass_hall.png";

    // The pet is happy when all of today's habits are done.
    private void UpdatePetLine()
    {
        var done = Habits.Count(h => h.IsDone);
        PetLine = Habits.Count > 0 && done == Habits.Count ? $"{PetName} on rõõmus!"
                : done > 0 ? $"{PetName} on juba tubli!"
                : $"{PetName} ootab sind!";
    }

    // Session expired -> back to login, other errors are shown on the screen.
    private async Task HandleErrorAsync(string error)
    {
        if (error.StartsWith("Sessioon aegus"))
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