using HarjumusteRPG.Mobile.ViewModels;

namespace HarjumusteRPG.Mobile.Views;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {

        BindingContext = viewModel;
    }
}