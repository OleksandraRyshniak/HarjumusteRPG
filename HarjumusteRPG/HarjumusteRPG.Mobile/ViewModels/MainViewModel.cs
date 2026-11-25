using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HarjumusteRPG.Mobile.Services;

namespace HarjumusteRPG.Mobile.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    private readonly IAuthService _auth;

    public MainViewModel(IAuthService auth)
    {
        _auth = auth;
        Title = "Pawly";
    }

    [ObservableProperty]
    private string greeting = string.Empty;

    [RelayCommand]
    private void Refresh() => Greeting = Loc.F(AppResources.GreetingFormat, _auth.Username);

    [RelayCommand]
    private async Task Logout()
    {
        _auth.Logout();
        await Shell.Current.GoToAsync("//login");
    }
}