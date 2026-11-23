using HarjumusteRPG.Mobile.ViewModels;

namespace HarjumusteRPG.Mobile.Views;

public partial class RegisterPage : ContentPage
{
    private readonly VerticalStackLayout _column;

    public RegisterPage(RegisterViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
        BackgroundColor = Palette.Bg;
        Shell.SetNavBarIsVisible(this, false);
        this.SetBinding(TitleProperty, nameof(RegisterViewModel.Title));

        var busy = new ActivityIndicator { Color = Palette.Ink };
        busy.SetBinding(ActivityIndicator.IsRunningProperty, nameof(RegisterViewModel.IsBusy));
        busy.SetBinding(IsVisibleProperty, nameof(RegisterViewModel.IsBusy));

        // Compact form: smaller gaps between the fields.
        var form = new VerticalStackLayout
        {
            Spacing = 8,
            Children =
            {
                Pixel.Heading("REGISTREERU", 14),
                Pixel.Field("Kasutajanimi (3–50 märki)", nameof(RegisterViewModel.Username)),
                Pixel.Field("E-post", nameof(RegisterViewModel.Email), keyboard: Keyboard.Email),
                Pixel.Field("Parool (vähemalt 6 märki)", nameof(RegisterViewModel.Password), password: true),
                Pixel.Field("Korda parooli", nameof(RegisterViewModel.ConfirmPassword), password: true,
                    returnType: ReturnType.Done, returnCommandPath: nameof(RegisterViewModel.RegisterCommand)),
                Pixel.ErrorLabel(nameof(RegisterViewModel.ErrorMessage), nameof(RegisterViewModel.HasError)),
                Pixel.PrimaryButton("Registreeru", nameof(RegisterViewModel.RegisterCommand),
                    enabledPath: nameof(RegisterViewModel.IsNotBusy)),
                busy
            }
        };

        _column = new VerticalStackLayout
        {
            Padding = new Thickness(24, 16),
            Spacing = 10,
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
                Pixel.Card(form, padding: new Thickness(12)),
                Pixel.LinkButton("Konto olemas? Logi sisse", nameof(RegisterViewModel.GoToLoginCommand))
            }
        };

        Content = new ScrollView { Content = _column };
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (width > 0)
            _column.WidthRequest = Math.Min(width, 420);
    }
}