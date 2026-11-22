using HarjumusteRPG.Mobile.ViewModels;

namespace HarjumusteRPG.Mobile.Views;

public partial class ProgressPage : ContentPage
{
    private readonly Grid _root;

    public ProgressPage(ProgressViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
        BackgroundColor = Palette.Bg;
        Shell.SetNavBarIsVisible(this, false);
        this.SetBinding(TitleProperty, nameof(ProgressViewModel.Title));

        var heading = Pixel.Heading("PROGRESS", 16);

        // --- pet card: sprite, name, level and XP bar ---
        var pet = new Image
        {
            WidthRequest = 84,
            HeightRequest = 114,
            Aspect = Aspect.AspectFit,
            VerticalOptions = LayoutOptions.Center
        };
        pet.SetBinding(Image.SourceProperty, nameof(ProgressViewModel.PetImage));

        var petName = Pixel.Body(string.Empty, 20, bold: true);
        petName.SetBinding(Label.TextProperty, nameof(ProgressViewModel.PetName));

        var level = Pixel.Heading(string.Empty, 14);
        level.SetBinding(Label.TextProperty, nameof(ProgressViewModel.LevelText));

        var xp = Pixel.Body(string.Empty, 16, bold: true);
        xp.SetBinding(Label.TextProperty, nameof(ProgressViewModel.XpText));

        var bar = new ProgressBar { ProgressColor = Palette.Ink, BackgroundColor = Palette.Card, HeightRequest = 14 };
        bar.SetBinding(ProgressBar.ProgressProperty, nameof(ProgressViewModel.XpFraction));

        var hint = Pixel.Body(string.Empty, 14, color: Palette.Sub);
        hint.SetBinding(Label.TextProperty, nameof(ProgressViewModel.Hint));

        var info = new VerticalStackLayout
        {
            Spacing = 6,
            VerticalOptions = LayoutOptions.Center,
            Children = { petName, level, xp, bar, hint }
        };

        var petGrid = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) },
            ColumnSpacing = 14
        };
        petGrid.Add(pet, 0);
        petGrid.Add(info, 1);

        var petCard = Pixel.Card(petGrid, Palette.Hero, padding: new Thickness(14));

        var tapPet = new TapGestureRecognizer();
        tapPet.SetBinding(TapGestureRecognizer.CommandProperty, nameof(TodayViewModel.ChangePetCommand));
        petCard.GestureRecognizers.Add(tapPet);
        // --- four numbers ---
        var stats = new Grid
        {
            ColumnSpacing = 12,
            RowSpacing = 12,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) },
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto) }
        };
        stats.Add(Tile("ic_tuli.png", nameof(ProgressViewModel.StreakText), "päeva seeria"), 0, 0);
        stats.Add(Tile("ic_taht.png", nameof(ProgressViewModel.LongestText), "pikim seeria"), 1, 0);
        stats.Add(Tile("ic_kalender.png", nameof(ProgressViewModel.TotalText), "täitmist"), 0, 1);
        stats.Add(Tile("ic_mynt.png", nameof(ProgressViewModel.CoinsText), "münti"), 1, 1);

        var error = Pixel.ErrorLabel(nameof(ProgressViewModel.ErrorMessage), nameof(ProgressViewModel.HasError));

        var busy = new ActivityIndicator { Color = Palette.Ink };
        busy.SetBinding(ActivityIndicator.IsRunningProperty, nameof(ProgressViewModel.IsBusy));
        busy.SetBinding(IsVisibleProperty, nameof(ProgressViewModel.IsBusy));

        var column = new VerticalStackLayout
        {
            Padding = new Thickness(20, 20, 20, 12),
            Spacing = 14,
            Children = { heading, petCard, stats, error, busy }
        };

        _root = new Grid
        {
            RowDefinitions = { new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto) },
            HorizontalOptions = LayoutOptions.Center
        };
        _root.Add(new ScrollView { Content = column }, 0, 0);
        _root.Add(NavBar.Build(0), 0, 1);

        Content = _root;
    }

    // Icon, big number and a short caption.
    private static Grid Tile(string icon, string valuePath, string caption)
    {
        var value = Pixel.Body(string.Empty, 24, bold: true);
        value.SetBinding(Label.TextProperty, valuePath);

        var text = new VerticalStackLayout
        {
            Spacing = 2,
            VerticalOptions = LayoutOptions.Center,
            Children = { value, Pixel.Body(caption, 14, color: Palette.Sub) }
        };

        var row = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) },
            ColumnSpacing = 10
        };
        row.Add(new Image { Source = icon, WidthRequest = 32, HeightRequest = 32, VerticalOptions = LayoutOptions.Center }, 0);
        row.Add(text, 1);

        return Pixel.Card(row, padding: new Thickness(12));
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (width > 0)
            _root.WidthRequest = Math.Min(width, 420);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is ProgressViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}