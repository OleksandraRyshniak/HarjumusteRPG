
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HarjumusteRPG.Mobile.Services;

namespace HarjumusteRPG.Mobile.ViewModels;

public partial class ProgressViewModel : BaseViewModel
{
    private readonly IAuthService _auth;
    private readonly IGameApi _api;

    public ProgressViewModel(IAuthService auth, IGameApi api)
    {
        _auth = auth;
        _api = api;
        Title = AppResources.NavPet;
    }

    [ObservableProperty] private string petImage = "kass_hall.png";
    [ObservableProperty] private string petName = string.Empty;
    [ObservableProperty] private string levelText = string.Empty;
    [ObservableProperty] private string moodText = string.Empty;
    [ObservableProperty] private bool isHappy;

    // needs as 0..1 for the bars
    [ObservableProperty] private double joyFraction;
    [ObservableProperty] private double foodFraction;
    [ObservableProperty] private double energyFraction;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPetMessage))]
    private string? petMessage;

    public bool HasPetMessage => !string.IsNullOrEmpty(PetMessage);

    public string FeedText => Loc.F(AppResources.PetFeedFormat, PetRules.FeedCost);

    // The page makes the pet jump when this fires.
    public event Action? PetReacted;

    [RelayCommand]
    private Task OpenAchievements() => Nav.GoAsync("achievements");

    [RelayCommand]
    private void Pat() => DoFree(PetAction.Pat, AppResources.PetPatMessage);

    [RelayCommand]
    private void Play() => DoFree(PetAction.Play, AppResources.PetPlayMessage);

    // Feed: checked first (no coins are taken if the pet is full), then paid on the server.
    [RelayCommand]
    private async Task Feed()
    {
        if (IsBusy || ShowBlocker(PetAction.Feed))
            return;

        IsBusy = true;
        try
        {
            ErrorMessage = null;
            PetMessage = null;

            var result = await _api.FeedPetAsync();
            if (!result.Success)
            {
                await HandleErrorAsync(result.Error!);
                return;
            }

            ShowMood(PetStore.Apply(_auth.Username, PetAction.Feed));
            PetMessage = Loc.F(AppResources.PetFedMessage, PetName, PetRules.FeedCost);
            PetReacted?.Invoke();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task Load()
    {
        PetImage = PetStore.Image(_auth.Username);
        PetName = PetStore.Name(_auth.Username);
        ShowMood(PetStore.Mood(_auth.Username));
        PetMessage = null;

        if (IsBusy)
            return;

        IsBusy = true;
        try
        {
            ErrorMessage = null;

            var result = await _api.GetProgressAsync();
            if (!result.Success)
            {
                await HandleErrorAsync(result.Error!);
                return;
            }

            LevelText = Loc.F(AppResources.LevelFormat, result.Value!.Level);
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Pat and play: free actions on the device.
    private void DoFree(PetAction action, string message)
    {
        if (ShowBlocker(action))
            return;

        ErrorMessage = null;
        ShowMood(PetStore.Apply(_auth.Username, action));
        PetMessage = Loc.F(message, PetName);
        PetReacted?.Invoke();
    }

    // Shows why the action is not possible now (hungry, tired, full).
    private bool ShowBlocker(PetAction action)
    {
        var mood = PetStore.Mood(_auth.Username);
        ShowMood(mood);

        var blocker = PetRules.Blocker(mood, action);
        if (blocker is null)
            return false;

        PetMessage = Loc.F(blocker, PetName);
        return true;
    }

    private void ShowMood(PetMood mood)
    {
        JoyFraction = mood.Joy / 100.0;
        FoodFraction = mood.Food / 100.0;
        EnergyFraction = mood.Energy / 100.0;

        var state = PetRules.State(mood);
        IsHappy = state == PetState.Happy;
        MoodText = state switch
        {
            PetState.Happy => AppResources.MoodHappy,
            PetState.Hungry => AppResources.MoodHungry,
            PetState.Tired => AppResources.MoodTired,
            PetState.Sad => AppResources.MoodSad,
            _ => AppResources.MoodCalm
        };
    }

    // Session expired -> back to login, other errors are shown on the screen.
    private async Task HandleErrorAsync(string error)
    {
        if (error.StartsWith(AppResources.ErrSessionExpired))
        {
            _auth.Logout();
            await Task.Delay(300);
            await Nav.GoAsync("//login");
            return;
        }
        ErrorMessage = error;
    }
}