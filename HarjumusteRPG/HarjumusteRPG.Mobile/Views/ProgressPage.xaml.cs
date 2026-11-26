using Microsoft.Maui.Controls.Shapes;
using HarjumusteRPG.Mobile.ViewModels;

namespace HarjumusteRPG.Mobile.Views;

public partial class ProgressPage : ContentPage
{
    private readonly Grid _root;
    private readonly View _petView;

    private static bool Night => Palette.ThemeName == "night";

    public ProgressPage(ProgressViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
        BackgroundColor = Palette.Bg;
        Shell.SetNavBarIsVisible(this, false);
        this.SetBinding(TitleProperty, nameof(ProgressViewModel.Title));

        var heading = Pixel.Heading(AppResources.PetTitle, 16);

        // --- room: window, floor, bowl, ball and the pet in the middle ---
        var room = new Grid { HeightRequest = 230, BackgroundColor = Color.FromArgb(Night ? "#2A2140" : "#FFE9D6") };

        room.Add(new BoxView
        {
            Color = Color.FromArgb(Night ? "#1A1326" : "#F4C9D6"),
            HeightRequest = 54,
            VerticalOptions = LayoutOptions.End
        });
        room.Add(new Border
        {
            Stroke = Palette.Ink,
            StrokeThickness = 4,
            StrokeShape = new Rectangle(),
            BackgroundColor = Color.FromArgb(Night ? "#0E0A18" : "#DDEBFA"),
            WidthRequest = 64,
            HeightRequest = 48,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Start,
            Margin = new Thickness(20, 20, 0, 0)
        });
        room.Add(new Image
        {
            Source = "kauss.png",
            WidthRequest = 44,
            HeightRequest = 44,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.End,
            Margin = new Thickness(24, 0, 0, 16)
        });
        room.Add(new Image
        {
            Source = "pall.png",
            WidthRequest = 34,
            HeightRequest = 34,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.End,
            Margin = new Thickness(0, 0, 26, 18)
        });

        _petView = BuildPet();
        room.Add(_petView);

        var petName = Pixel.Heading(string.Empty, 13);
        petName.SetBinding(Label.TextProperty, nameof(ProgressViewModel.PetName));

        var level = Pixel.Body(string.Empty, 16, color: Palette.Sub);
        level.SetBinding(Label.TextProperty, nameof(ProgressViewModel.LevelText));

        var mood = Pixel.Body(string.Empty, 16, bold: true, center: true);
        mood.VerticalOptions = LayoutOptions.Center;
        mood.SetBinding(Label.TextProperty, nameof(ProgressViewModel.MoodText));
        var moodChip = Pixel.Card(mood, Palette.Mint, border: 3, shadow: 3, padding: new Thickness(10, 4));
        moodChip.VerticalOptions = LayoutOptions.Center;

        var nameRow = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            Padding = new Thickness(14, 10),
            ColumnSpacing = 8
        };
        nameRow.Add(new VerticalStackLayout { Spacing = 4, Children = { petName, level } }, 0);
        nameRow.Add(moodChip, 1);

        var roomCard = Pixel.Card(new VerticalStackLayout
        {
            Children = { room, new BoxView { Color = Palette.Ink, HeightRequest = 4 }, nameRow }
        }, padding: new Thickness(0));

        // --- three actions ---
        var actions = new Grid
        {
            ColumnSpacing = 10,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            }
        };
        actions.Add(Ui.CommandButton(AppResources.PetPat, nameof(ProgressViewModel.PatCommand)), 0);
        actions.Add(Ui.CommandButton(AppResources.PetPlay, nameof(ProgressViewModel.PlayCommand)), 1);
        actions.Add(Ui.CommandButton(viewModel.FeedText, nameof(ProgressViewModel.FeedCommand), fill: Palette.Hero), 2);

        var message = Pixel.Body(string.Empty, 17, bold: true);
        message.SetBinding(Label.TextProperty, nameof(ProgressViewModel.PetMessage));
        message.SetBinding(IsVisibleProperty, nameof(ProgressViewModel.HasPetMessage));

        var error = Pixel.ErrorLabel(nameof(ProgressViewModel.ErrorMessage), nameof(ProgressViewModel.HasError));

        // --- three bars ---
        var bars = Pixel.Card(new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                NeedRow("ic_suda.png", AppResources.PetJoy, nameof(ProgressViewModel.JoyFraction), Palette.Pink),
                NeedRow("ic_kauss.png", AppResources.PetFood, nameof(ProgressViewModel.FoodFraction), Palette.Gold),
                NeedRow("ic_valk.png", AppResources.PetEnergy, nameof(ProgressViewModel.EnergyFraction), Palette.Mint)
            }
        }, padding: new Thickness(14));

        var info = Pixel.Body(AppResources.PetInfo, 15, color: Palette.Sub);

        var column = new VerticalStackLayout
        {
            Padding = new Thickness(20, 20, 20, 12),
            Spacing = 14,
            Children =
            {
                Header.Row(heading),
                roomCard,
                actions,
                message,
                error,
                bars,
                info,
                Pixel.PrimaryButton(AppResources.Achievements, nameof(ProgressViewModel.OpenAchievementsCommand), primary: false)
            }
        };

        _root = new Grid
        {
            RowDefinitions = { new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto) },
            HorizontalOptions = LayoutOptions.Center
        };
        _root.Add(new ScrollView { Content = column }, 0, 0);
        _root.Add(NavBar.Build(1), 0, 1);

        viewModel.PetReacted += () => _ = JumpAsync();

        Content = _root;
    }

    // Heart (when happy) above the pet.
    private static View BuildPet()
    {
        var heart = new Image { Source = "ic_suda.png", WidthRequest = 28, HeightRequest = 28, HorizontalOptions = LayoutOptions.Center };
        heart.SetBinding(IsVisibleProperty, nameof(ProgressViewModel.IsHappy));

        var pet = new Image
        {
            WidthRequest = 120,
            HeightRequest = 120,
            Aspect = Aspect.AspectFit,
            HorizontalOptions = LayoutOptions.Center
        };
        pet.SetBinding(Image.SourceProperty, nameof(ProgressViewModel.PetImage));

        return new VerticalStackLayout
        {
            Spacing = 6,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.End,
            Margin = new Thickness(0, 0, 0, 18),
            Children = { heart, pet }
        };
    }

    // Icon, name of the need and a bar.
    private static Grid NeedRow(string icon, string text, string valuePath, Color color)
    {
        var label = Pixel.Body(text, 16, bold: true);
        label.VerticalOptions = LayoutOptions.Center;
        label.WidthRequest = 80;

        var bar = new ProgressBar
        {
            ProgressColor = color,
            BackgroundColor = Palette.Bg,
            HeightRequest = 14,
            VerticalOptions = LayoutOptions.Center
        };
        bar.SetBinding(ProgressBar.ProgressProperty, valuePath);

        var row = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            },
            ColumnSpacing = 10
        };
        row.Add(new Image { Source = icon, WidthRequest = 24, HeightRequest = 24, VerticalOptions = LayoutOptions.Center }, 0);
        row.Add(label, 1);
        row.Add(bar, 2);
        return row;
    }

    // The pet jumps a little after an action.
    private async Task JumpAsync()
    {
        await _petView.TranslateTo(0, -20, 150, Easing.CubicOut);
        await _petView.TranslateTo(0, 0, 300, Easing.BounceOut);
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