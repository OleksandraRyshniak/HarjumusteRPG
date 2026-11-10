using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace HarjumusteRPG.Mobile.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    [ObservableProperty]
    private string greeting = "Harjumuste RPG";

    public MainViewModel() => Title = "Home";

    [RelayCommand]
    private void ChangeGreeting() => Greeting = "MVVM works!";
}