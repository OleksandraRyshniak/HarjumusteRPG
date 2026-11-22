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
        Title = "Progress";
    }

    [ObservableProperty] private string petImage = "kass_hall.png";
    [ObservableProperty] private string petName = string.Empty;
    [ObservableProperty] private string levelText = "TASE 1";
    [ObservableProperty] private string xpText = "0/100 XP";
    [ObservableProperty] private double xpFraction;
    [ObservableProperty] private string hint = string.Empty;
    [ObservableProperty] private string streakText = "0";
    [ObservableProperty] private string longestText = "0";
    [ObservableProperty] private string totalText = "0";
    [ObservableProperty] private string coinsText = "0";

    [RelayCommand]
    private async Task Load()
    {
        PetImage = PetStore.Image(_auth.Username);
        PetName = PetStore.Name(_auth.Username);

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

            var p = result.Value!;
            LevelText = $"TASE {p.Level}";
            XpText = $"{p.Xp}/{p.XpForNextLevel} XP";
            XpFraction = p.XpForNextLevel > 0 ? Math.Clamp((double)p.Xp / p.XpForNextLevel, 0, 1) : 0;
            Hint = $"Järgmise tasemeni veel {Math.Max(p.XpForNextLevel - p.Xp, 0)} XP";
            StreakText = p.CurrentStreak.ToString();
            LongestText = p.LongestStreak.ToString();
            TotalText = p.TotalCompletions.ToString();
            CoinsText = p.Coins.ToString();
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Session expired -> back to login, other errors are shown on the screen.
    private async Task HandleErrorAsync(string error)
    {
        if (error.StartsWith("Sessioon aegus"))
        {
            _auth.Logout();
            await Task.Delay(300);
            await Shell.Current.GoToAsync("//login");
            return;
        }
        ErrorMessage = error;
    }
}