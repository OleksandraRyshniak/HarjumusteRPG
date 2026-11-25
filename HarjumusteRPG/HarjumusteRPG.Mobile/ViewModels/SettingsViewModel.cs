using CommunityToolkit.Mvvm.Input;
using HarjumusteRPG.Mobile.Views;

namespace HarjumusteRPG.Mobile.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    public SettingsViewModel()
    {
        Title = AppResources.Settings;
    }

    public bool IsEstonian => !Loc.IsEnglish;
    public bool IsEnglish => Loc.IsEnglish;
    public bool IsCandy => Palette.ThemeName == "candy";
   
    public bool IsNight => Palette.ThemeName == "night";

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

    [RelayCommand]
    private Task Back() => Shell.Current.GoToAsync("..");

    // Pages are built in code with the current colors and texts,
    // so a new shell is created to apply the change everywhere.
    private static void Reload()
    {
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window is not null)
            window.Page = new AppShell();
    }
}