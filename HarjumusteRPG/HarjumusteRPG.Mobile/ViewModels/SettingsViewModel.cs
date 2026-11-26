using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HarjumusteRPG.Mobile.Models;
using HarjumusteRPG.Mobile.Services;
using HarjumusteRPG.Mobile.Views;

namespace HarjumusteRPG.Mobile.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly IGameApi _api;
    private readonly IAuthService _auth;

    public SettingsViewModel(IGameApi api, IAuthService auth)
    {
        _api = api;
        _auth = auth;
        Title = AppResources.Settings;
    }

    public bool IsEstonian => !Loc.IsEnglish;
    public bool IsEnglish => Loc.IsEnglish;
    public bool IsCandy => Palette.ThemeName == "candy";
    public bool IsNight => Palette.ThemeName == "night";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDemoText))]
    private string? demoText;

    public bool HasDemoText => !string.IsNullOrEmpty(DemoText);

    [RelayCommand]
    private void SetLanguage(string? code)
    {
        if (code is null || code == Loc.Language)
            return;

        Loc.SetLanguage(code);
        Reload();
    }

    [RelayCommand]
    private void SetTheme(string? name)
    {
        if (name is null || name == Palette.ThemeName)
            return;

        Palette.SetTheme(name);
        Reload();
    }

    // Demo: the API clock goes one day forward and the streak check runs.
    [RelayCommand]
    private async Task NextDay()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        try
        {
            ErrorMessage = null;
            var result = await _api.DemoNextDayAsync();
            if (!result.Success)
            {
                ErrorMessage = result.Error;
                return;
            }

            PetStore.SkipHours(_auth.Username, 24);
            ShowDay(result.Value!);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ResetTime()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        try
        {
            ErrorMessage = null;
            var result = await _api.DemoResetAsync();
            if (!result.Success)
            {
                ErrorMessage = result.Error;
                return;
            }

            ShowDay(result.Value!);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ShowDay(DemoTimeResult r) =>
        DemoText = Loc.F(AppResources.DemoDayFormat,
            r.Today.ToDateTime(TimeOnly.MinValue).ToString("d. MMM", Loc.Culture), r.StreaksReset);

    [RelayCommand]
    private async Task Logout()
    {
        _auth.Logout();
        await Nav.GoAsync("//login");
    }

    [RelayCommand]
    private Task Back() => Nav.GoAsync("..");

    // Pages are built in code with the current colors and texts,
    // so a new shell is created to apply the change everywhere.
    private static void Reload()
    {
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window is not null)
            window.Page = new AppShell();
    }
}