using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HarjumusteRPG.Mobile.Models;
using HarjumusteRPG.Mobile.Services;
using System.Collections.ObjectModel;
using System.Globalization;

namespace HarjumusteRPG.Mobile.ViewModels;

// One line of the achievements list. Opacity is 1 for earned and 0.5 for locked ones.
public record AchievementRow(string Name, string Description, string Status, bool Unlocked, double Opacity);

public partial class AchievementsViewModel : BaseViewModel
{
    private static CultureInfo Estonian => Loc.Culture;

    private readonly IAuthService _auth;
    private readonly IGameApi _api;

    public AchievementsViewModel(IAuthService auth, IGameApi api)
    {
        _auth = auth;
        _api = api;
        Title = AppResources.Achievements;
    }

    public ObservableCollection<AchievementRow> Rows { get; } = new();

    [ObservableProperty]
    private string summary = string.Empty;

    [RelayCommand]
    private async Task Load()
    {
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

            var all = result.Value!.Achievements ?? new();

            Rows.Clear();
            foreach (var a in all.OrderByDescending(a => a.Unlocked).ThenBy(a => a.Id))
            {
                var status = a.Unlocked
                    ? Loc.F(AppResources.EarnedFormat, (a.UnlockedAt ?? DateTime.UtcNow).ToLocalTime().ToString("d. MMM yyyy", Estonian))
                    : AppResources.Locked;
                Rows.Add(new AchievementRow(AchievementTexts.Name(a), AchievementTexts.Description(a), status, a.Unlocked, a.Unlocked ? 1 : 0.5));
            }

            Summary = Loc.F(AppResources.EarnedSummaryFormat, all.Count(a => a.Unlocked), all.Count);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task Back() => Shell.Current.GoToAsync("..");

    // Session expired -> back to login, other errors are shown on the screen.
    private async Task HandleErrorAsync(string error)
    {
        if (error.StartsWith(AppResources.ErrSessionExpired))
        {
            _auth.Logout();
            await Task.Delay(300);
            await Shell.Current.GoToAsync("//login");
            return;
        }
        ErrorMessage = error;
    }
}