
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HarjumusteRPG.Mobile.Services;
using System.Runtime.CompilerServices;


namespace HarjumusteRPG.Mobile.ViewModels;

public partial class ProgressViewModel : BaseViewModel
{
    // same as GameRules.FeedCost in the API
    private const int FeedCost = 5;

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

    public string FeedText => Loc.F(AppResources.PetFeedFormat, FeedCost);

    // The page makes the pet jump when this fires.
    public event Action? PetReacted;

    [RelayCommand]
    private Task OpenAchievements() => Nav.GoAsync("achievements");

    // Pat: free, a little joy.
    [RelayCommand]
    private void Pat()
    {
        ShowMood(PetStore.ChangeMood(_auth.Username, joy: 5));
        PetMessage = Loc.F(AppResources.PetPatMessage, PetName);
        PetReacted?.Invoke();
    }

    // Play: free, more joy but uses energy.
    [RelayCommand]
    private void Play()
    {
        if (PetStore.Mood(_auth.Username).Energy < 10)
        {
            PetMessage = Loc.F(AppResources.PetTiredMessage, PetName);
            return;
        }

        ShowMood(PetStore.ChangeMood(_auth.Username, joy: 8, energy: -10));
        PetMessage = Loc.F(AppResources.PetPlayMessage, PetName);
        PetReacted?.Invoke();
    }

    // Feed: costs coins on the server, fills food and gives energy.
    [RelayCommand]
    private async Task Feed()
    {
        if (IsBusy)
            return;

        if (PetStore.Mood(_auth.Username).Food >= 100)
        {
            PetMessage = Loc.F(AppResources.PetFullMessage, PetName);
            return;
        }

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

            ShowMood(PetStore.ChangeMood(_auth.Username, food: 20, energy: 10));
            PetMessage = Loc.F(AppResources.PetFedMessage, PetName, FeedCost);
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

    

    private void ShowMood(PetMood mood)
    {
        JoyFraction = mood.Joy / 100.0;
        FoodFraction = mood.Food / 100.0;
        EnergyFraction = mood.Energy / 100.0;

        IsHappy = mood.Joy >= 70;
        MoodText = mood.Joy >= 70 ? AppResources.MoodHappy
            : mood.Joy >= 40 ? AppResources.MoodCalm
            : AppResources.MoodSad;
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