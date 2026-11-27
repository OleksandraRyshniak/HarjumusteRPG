using HarjumusteRPG.Mobile.ViewModels;

namespace HarjumusteRPG.Mobile.Views;

public partial class LoginPage : ContentPage
{
    private readonly VerticalStackLayout _column;

    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
        BackgroundColor = Palette.Bg;
        Shell.SetNavBarIsVisible(this, false);
        this.SetBinding(TitleProperty, nameof(LoginViewModel.Title));

        var busy = new ActivityIndicator { Color = Palette.Ink };
        busy.SetBinding(ActivityIndicator.IsRunningProperty, nameof(LoginViewModel.IsBusy));
        busy.SetBinding(IsVisibleProperty, nameof(LoginViewModel.IsBusy));

        var form = new VerticalStackLayout
        {
            Spacing = 14,
            Children =
            {
                Pixel.Heading(AppResources.LoginTitle, 14),
                Pixel.Field(AppResources.Email, nameof(LoginViewModel.Email), keyboard: Keyboard.Email),
                Pixel.Field(AppResources.Password, nameof(LoginViewModel.Password), password: true,
                    returnType: ReturnType.Done, returnCommandPath: nameof(LoginViewModel.LoginCommand)),
                Pixel.ErrorLabel(nameof(LoginViewModel.ErrorMessage), nameof(LoginViewModel.HasError)),
                Pixel.PrimaryButton(AppResources.LoginButton, nameof(LoginViewModel.LoginCommand),
                    enabledPath: nameof(LoginViewModel.IsNotBusy)),
                busy
            }
        };

        _column = new VerticalStackLayout
        {
            Padding = new Thickness(24, 32),
            Spacing = 18,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Image
                {
                    Source = "pawly_logo.png",
                    WidthRequest = 120,
                    HeightRequest = 120,
                    HorizontalOptions = LayoutOptions.Center
                },
                Pixel.Heading("PAWLY", 24, center: true),
                Pixel.Body("Harjumuste RPG", 18, color: Palette.Sub, center: true),
                Pixel.Card(form),
                Pixel.LinkButton(AppResources.GoRegister, nameof(LoginViewModel.GoToRegisterCommand))
            }
        };

        Content = new ScrollView { Content = _column };
    }

    // The column is at most 420 wide and centered on wide windows.
    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (width > 0)
            _column.WidthRequest = Math.Min(width, 420);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Let Shell finish showing the login page before navigating away from it.
        await Task.Delay(200);

        // If a saved session exists, skip the login screen.
        if (BindingContext is LoginViewModel vm)
            await vm.RestoreSessionCommand.ExecuteAsync(null);
    }
}