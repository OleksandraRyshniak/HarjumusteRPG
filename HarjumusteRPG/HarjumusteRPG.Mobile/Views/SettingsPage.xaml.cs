using HarjumusteRPG.Mobile.ViewModels;

namespace HarjumusteRPG.Mobile.Views;

public partial class SettingsPage : ContentPage
{
    private readonly VerticalStackLayout _column;

    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
        BackgroundColor = Palette.Bg;
        Shell.SetNavBarIsVisible(this, false);
        this.SetBinding(TitleProperty, nameof(SettingsViewModel.Title));

        var back = Ui.CommandButton(AppResources.Back, nameof(SettingsViewModel.BackCommand));
        back.HorizontalOptions = LayoutOptions.Start;

        // --- language ---
        var languages = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) }
        };
        languages.Add(Ui.CommandButton("Eesti", nameof(SettingsViewModel.SetLanguageCommand), "et",
            nameof(SettingsViewModel.IsEstonian)), 0);
        languages.Add(Ui.CommandButton("English", nameof(SettingsViewModel.SetLanguageCommand), "en",
            nameof(SettingsViewModel.IsEnglish)), 1);

        var languageCard = Pixel.Card(new VerticalStackLayout
        {
            Spacing = 12,
            Children = { Pixel.Heading(AppResources.LanguageTitle, 12), languages }
        });

        // --- theme ---
        var themes = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) }
        };
        themes.Add(Ui.CommandButton(AppResources.ThemeCandy, nameof(SettingsViewModel.SetThemeCommand), "candy",
            nameof(SettingsViewModel.IsCandy)), 0);
        themes.Add(Ui.CommandButton(AppResources.ThemeNight, nameof(SettingsViewModel.SetThemeCommand), "night",
            nameof(SettingsViewModel.IsNight)), 1);

        var themeCard = Pixel.Card(new VerticalStackLayout
        {
            Spacing = 12,
            Children = { Pixel.Heading(AppResources.ThemeTitle, 12), themes }
        });
        // --- demo: move the API clock forward to show the streak ---
        var demoText = Pixel.Body(string.Empty, 16, bold: true);
        demoText.SetBinding(Label.TextProperty, nameof(SettingsViewModel.DemoText));
        demoText.SetBinding(IsVisibleProperty, nameof(SettingsViewModel.HasDemoText));

        var demoError = Pixel.ErrorLabel(nameof(SettingsViewModel.ErrorMessage), nameof(SettingsViewModel.HasError));

        var demoButtons = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) }
        };
        demoButtons.Add(Ui.CommandButton(AppResources.DemoNextDay, nameof(SettingsViewModel.NextDayCommand), fill: Palette.Hero), 0);
        demoButtons.Add(Ui.CommandButton(AppResources.DemoReset, nameof(SettingsViewModel.ResetTimeCommand)), 1);

        var demoCard = Pixel.Card(new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                Pixel.Heading(AppResources.DemoTitle, 12),
                Pixel.Body(AppResources.DemoHint, 14, color: Palette.Sub),
                demoButtons,
                demoText,
                demoError
            }
        });

        _column = new VerticalStackLayout
        {
            Padding = new Thickness(20),
            Spacing = 16,
            HorizontalOptions = LayoutOptions.Center,
            Children =
            {
                back,
                Pixel.Heading(AppResources.SettingsTitle, 16),
                languageCard,
                themeCard,
                demoCard,
                Pixel.PrimaryButton(AppResources.Logout, nameof(SettingsViewModel.LogoutCommand), primary: false)
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