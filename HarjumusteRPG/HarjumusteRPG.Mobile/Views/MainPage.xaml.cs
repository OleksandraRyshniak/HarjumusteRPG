using HarjumusteRPG.Mobile.ViewModels;

namespace HarjumusteRPG.Mobile.Views;

public partial class MainPage : ContentPage
{
    private readonly VerticalStackLayout _column;

    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
        BackgroundColor = Palette.Bg;
        Shell.SetNavBarIsVisible(this, false);
        this.SetBinding(TitleProperty, nameof(MainViewModel.Title));

        var greeting = Pixel.Heading(string.Empty, 14, center: true);
        greeting.SetBinding(Label.TextProperty, nameof(MainViewModel.Greeting));

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
                greeting,
                Pixel.Body("Siia tuleb tänase päeva ekraan.", 17, color: Palette.Sub, center: true),
                Pixel.PrimaryButton("Logi välja", nameof(MainViewModel.LogoutCommand), primary: false)
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

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is MainViewModel vm)
            vm.RefreshCommand.Execute(null);
    }
}