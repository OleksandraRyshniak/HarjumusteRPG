using HarjumusteRPG.Mobile.ViewModels;

namespace HarjumusteRPG.Mobile.Views;

public partial class TodayPage : ContentPage
{

    private static Color Mint => Palette.Mint;
    private static Color Pink => Palette.Pink;
    private static Color Gold => Palette.Gold;
    private static readonly Random Rng = new();

    private readonly Grid _root;
    private Grid _overlay = null!;
    private View _overlayCard = null!;
    private Image _star = null!;
    private AbsoluteLayout _confetti = null!;

    public TodayPage(TodayViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
        BackgroundColor = Palette.Bg;
        Shell.SetNavBarIsVisible(this, false);
        this.SetBinding(TitleProperty, nameof(TodayViewModel.Title));

        // --- header: date + greeting on the left, coins and level on the right ---
        var date = Pixel.Body(string.Empty, 14, color: Palette.Sub);
        date.SetBinding(Label.TextProperty, nameof(TodayViewModel.DateText));

        var greeting = Pixel.Heading(string.Empty, 16);
        greeting.SetBinding(Label.TextProperty, nameof(TodayViewModel.Greeting));

        var title = new VerticalStackLayout { Spacing = 6, Children = { date, greeting } };

        var chips = new HorizontalStackLayout
        {
            Spacing = 8,
            VerticalOptions = LayoutOptions.Start,
            Children =
            {
                Chip(nameof(TodayViewModel.CoinsText), Palette.Card, "ic_mynt.png"),
                Chip(nameof(TodayViewModel.LevelChip), Mint, "ic_taht.png"),
                Header.SettingsButton()
            }
        };

        var header = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            ColumnSpacing = 8
        };
        header.Add(title, 0);
        header.Add(chips, 1);

        // --- pet card: sprite, mood, streak and XP bar ---
        var pet = new Image
        {
            WidthRequest = 84,
            HeightRequest = 114,
            Aspect = Aspect.AspectFit,
            VerticalOptions = LayoutOptions.Center
        };
        pet.SetBinding(Image.SourceProperty, nameof(TodayViewModel.PetImage));

        var petLine = Pixel.Body(string.Empty, 20, bold: true);
        petLine.SetBinding(Label.TextProperty, nameof(TodayViewModel.PetLine));

        var streak = Pixel.Body(string.Empty, 16, color: Palette.Sub);
        streak.SetBinding(Label.TextProperty, nameof(TodayViewModel.StreakLine));

        var xpText = Pixel.Body(string.Empty, 16, bold: true);
        xpText.SetBinding(Label.TextProperty, nameof(TodayViewModel.XpText));

        var xpBar = new ProgressBar { ProgressColor = Palette.Ink, BackgroundColor = Palette.Card, HeightRequest = 14 };
        xpBar.SetBinding(ProgressBar.ProgressProperty, nameof(TodayViewModel.XpFraction));

        var petInfo = new VerticalStackLayout
        {
            Spacing = 6,
            VerticalOptions = LayoutOptions.Center,
            Children = { petLine, IconRow("ic_tuli.png", streak), IconRow("ic_valk.png", xpText), xpBar }
        };

        var petGrid = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) },
            ColumnSpacing = 14
        };
        petGrid.Add(pet, 0);
        petGrid.Add(petInfo, 1);

        var petCard = Pixel.Card(petGrid, Palette.Hero, padding: new Thickness(14));

        // --- messages ---
        var section = Pixel.Body(string.Empty, 18, bold: true);
        section.VerticalOptions = LayoutOptions.Center;
        section.SetBinding(Label.TextProperty, nameof(TodayViewModel.SectionTitle));

        var add = Ui.CommandButton("+", nameof(TodayViewModel.AddHabitCommand), fill: Palette.Hero);
        add.WidthRequest = 48;

        var sectionRow = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }
        };
        sectionRow.Add(section, 0);
        sectionRow.Add(add, 1);

        var reward = Pixel.Body(string.Empty, 17, bold: true);
        reward.SetBinding(Label.TextProperty, nameof(TodayViewModel.RewardMessage));
        reward.SetBinding(IsVisibleProperty, nameof(TodayViewModel.HasReward));

        var error = Pixel.ErrorLabel(nameof(TodayViewModel.ErrorMessage), nameof(TodayViewModel.HasError));

        var empty = Pixel.Body(AppResources.TodayEmpty, 17, color: Palette.Sub);
        empty.SetBinding(IsVisibleProperty, nameof(TodayViewModel.IsEmpty));

        var busy = new ActivityIndicator { Color = Palette.Ink };
        busy.SetBinding(ActivityIndicator.IsRunningProperty, nameof(TodayViewModel.IsBusy));
        busy.SetBinding(IsVisibleProperty, nameof(TodayViewModel.IsBusy));

        // --- habit list (a stack, so the whole page scrolls as one) ---
        var list = new VerticalStackLayout { Spacing = 12 };
        BindableLayout.SetItemsSource(list, viewModel.Habits);
        BindableLayout.SetItemTemplate(list, new DataTemplate(CreateRow));

        var column = new VerticalStackLayout
        {
            Padding = new Thickness(20, 20, 20, 12),
            Spacing = 14,
            Children =
            {
                header,
                petCard,
                BuildGiftCard(),
                sectionRow,
                reward,
                error,
                busy,
                list,
                empty,
                Pixel.PrimaryButton(AppResources.ManageHabits, nameof(TodayViewModel.ManageCommand), primary: false)

            }
        };

        _root = new Grid
        {
            RowDefinitions = { new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto) },
            HorizontalOptions = LayoutOptions.Center
        };
        _root.Add(new ScrollView { Content = column }, 0, 0);
        _root.Add(NavBar.Build(0), 0, 1);

        // --- level-up / achievement window on top of everything ---
        _overlay = BuildCelebration();
        Grid.SetRowSpan(_overlay, 2);
        _root.Add(_overlay, 0, 0);

        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(TodayViewModel.IsCelebrating) && viewModel.IsCelebrating)
                _ = PlayCelebrationAsync();
        };

        Content = _root;
    }

    // Daily gift: icon, texts and a button that is active until the gift is claimed.
    private static Grid BuildGiftCard()
    {
        var texts = new VerticalStackLayout
        {
            Spacing = 2,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                Pixel.Body(AppResources.GiftTitle, 18, bold: true),
                Pixel.Body(AppResources.GiftHint, 14, color: Palette.Sub)
            }
        };

        var claim = Ui.CommandButton("+20", nameof(TodayViewModel.ClaimGiftCommand), fill: Palette.Hero);
        claim.SetBinding(VisualElement.IsVisibleProperty, nameof(TodayViewModel.GiftAvailable));

        var doneButton = Ui.MakeButton(AppResources.GiftClaimed, Palette.Card, 14);
        doneButton.IsEnabled = false;
        var claimed = Ui.WithShadow(doneButton);
        claimed.SetBinding(VisualElement.IsVisibleProperty, nameof(TodayViewModel.GiftClaimed));

        var action = new Grid { VerticalOptions = LayoutOptions.Center, Children = { claim, claimed } };

        var row = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 12
        };
        row.Add(new Image { Source = "ic_kingitus.png", WidthRequest = 32, HeightRequest = 32, VerticalOptions = LayoutOptions.Center }, 0);
        row.Add(texts, 1);
        row.Add(action, 2);

        return Pixel.Card(row, padding: new Thickness(12));
    }

    // Dark background, a card with a star, texts and a button, and confetti above.
    private Grid BuildCelebration()
    {
        var dim = new BoxView { Color = Colors.Black, Opacity = 0.55 };

        _star = new Image
        {
            Source = "ic_taht.png",
            WidthRequest = 96,
            HeightRequest = 96,
            Aspect = Aspect.AspectFit,
            HorizontalOptions = LayoutOptions.Center
        };

        var title = Pixel.Heading(string.Empty, 16, center: true);
        title.SetBinding(Label.TextProperty, nameof(TodayViewModel.CelebrationTitle));

        var level = Pixel.Heading(string.Empty, 28, center: true);
        level.SetBinding(Label.TextProperty, nameof(TodayViewModel.CelebrationLevel));
        level.SetBinding(IsVisibleProperty, nameof(TodayViewModel.HasCelebrationLevel));

        var rewardText = Pixel.Body(string.Empty, 18, bold: true, color: Palette.Sub, center: true);
        rewardText.SetBinding(Label.TextProperty, nameof(TodayViewModel.CelebrationReward));

        var achievements = Pixel.Body(string.Empty, 17, bold: true, center: true);
        achievements.SetBinding(Label.TextProperty, nameof(TodayViewModel.CelebrationAchievements));
        achievements.SetBinding(IsVisibleProperty, nameof(TodayViewModel.HasCelebrationAchievements));

        var close = Pixel.PrimaryButton(AppResources.Continue, nameof(TodayViewModel.CloseCelebrationCommand));

        var stack = new VerticalStackLayout
        {
            Spacing = 12,
            Children = { _star, title, level, rewardText, achievements, close }
        };

        var card = Pixel.Card(stack, Palette.Hero, padding: new Thickness(20));
        card.WidthRequest = 300;
        card.HorizontalOptions = LayoutOptions.Center;
        card.VerticalOptions = LayoutOptions.Center;
        _overlayCard = card;

        // confetti does not block the button
        _confetti = new AbsoluteLayout { InputTransparent = true };

        var overlay = new Grid { IsVisible = false };
        overlay.SetBinding(IsVisibleProperty, nameof(TodayViewModel.IsCelebrating));
        overlay.Add(dim);
        overlay.Add(card);
        overlay.Add(_confetti);
        return overlay;
    }

    // Card pops up, the star spins, confetti falls.
    private async Task PlayCelebrationAsync()
    {
        _overlayCard.Scale = 0.3;
        _overlayCard.Opacity = 0;
        _star.Rotation = 0;
        _star.Scale = 1;

        LaunchConfetti();

        await Task.WhenAll(
            _overlayCard.FadeTo(1, 150),
            _overlayCard.ScaleTo(1, 450, Easing.SpringOut));

        await _star.RotateTo(360, 700, Easing.CubicInOut);
        _star.Rotation = 0;
        await _star.ScaleTo(1.25, 200);
        await _star.ScaleTo(1, 200);
    }

    private void LaunchConfetti()
    {
        _confetti.Children.Clear();

        var width = _root.Width > 0 ? _root.Width : 360;
        var height = _root.Height > 0 ? _root.Height : 640;
        Color[] colors = { Pink, Mint, Palette.Hero, Gold };

        for (var i = 0; i < 28; i++)
        {
            var size = Rng.Next(8, 15);
            var piece = new BoxView
            {
                Color = colors[Rng.Next(colors.Length)],
                WidthRequest = size,
                HeightRequest = size
            };
            AbsoluteLayout.SetLayoutBounds(piece, new Rect(Rng.NextDouble() * (width - size), -20, size, size));
            _confetti.Children.Add(piece);
            _ = FallAsync(piece, height);
        }
    }

    private static async Task FallAsync(BoxView piece, double height)
    {
        await Task.Delay(Rng.Next(0, 600));
        var duration = (uint)Rng.Next(1400, 2600);
        await Task.WhenAll(
            piece.TranslateTo(Rng.Next(-40, 40), height + 40, duration, Easing.CubicIn),
            piece.RotateTo(Rng.Next(180, 720), duration));
    }

    // Small box in the header: icon + coins or level.
    private static Grid Chip(string path, Color fill, string icon)
    {
        var label = Pixel.Body(string.Empty, 18, bold: true, center: true);
        label.VerticalOptions = LayoutOptions.Center;
        label.SetBinding(Label.TextProperty, path);

        var content = new HorizontalStackLayout
        {
            Spacing = 6,
            Children =
            {
                new Image { Source = icon, WidthRequest = 24, HeightRequest = 24, VerticalOptions = LayoutOptions.Center },
                label
            }
        };
        return Pixel.Card(content, fill, border: 3, shadow: 3, padding: new Thickness(10, 6));
    }

    private static Grid IconRow(string icon, View content)
    {
        var grid = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) },
            ColumnSpacing = 6
        };
        grid.Add(new Image { Source = icon, WidthRequest = 20, HeightRequest = 20, VerticalOptions = LayoutOptions.Center }, 0);
        grid.Add(content, 1);
        return grid;
    }

    // One habit card: icon box, name, reward and a checkbox that marks the habit done.
    private View CreateRow()
    {
        var initial = Pixel.Body(string.Empty, 20, bold: true, center: true);
        initial.VerticalOptions = LayoutOptions.Center;
        initial.SetBinding(Label.TextProperty, nameof(TodayHabitItem.Initial));
        initial.SetBinding(IsVisibleProperty, nameof(TodayHabitItem.HasNoIcon));

        var picture = new Image
        {
            WidthRequest = 28,
            HeightRequest = 28,
            Aspect = Aspect.AspectFit,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        picture.SetBinding(Image.SourceProperty, nameof(TodayHabitItem.IconFile));
        picture.SetBinding(IsVisibleProperty, nameof(TodayHabitItem.HasIcon));

        var iconContent = new Grid { Children = { initial, picture } };
        var icon = Pixel.Card(iconContent, Palette.Bg, border: 3, shadow: 3, padding: new Thickness(0));
        icon.WidthRequest = 44;
        icon.HeightRequest = 44;
        icon.VerticalOptions = LayoutOptions.Center;

        var name = Pixel.Body(string.Empty, 19, bold: true);
        name.LineBreakMode = LineBreakMode.WordWrap;
        name.SetBinding(Label.TextProperty, nameof(TodayHabitItem.Name));

        var subtitle = Pixel.Body(string.Empty, 14, color: Palette.Sub);
        subtitle.SetBinding(Label.TextProperty, nameof(TodayHabitItem.Subtitle));

        var texts = new VerticalStackLayout
        {
            Spacing = 2,
            VerticalOptions = LayoutOptions.Center,
            Children = { name, subtitle }
        };

        var check = new Button
        {
            FontFamily = Pixel.TitleFont,
            FontSize = 8,
            TextColor = Palette.Ink,
            BorderColor = Palette.Ink,
            BorderWidth = 3,
            CornerRadius = 0,
            Padding = new Thickness(0),
            WidthRequest = 44,
            HeightRequest = 44,
            Margin = new Thickness(0, 0, 3, 3)
        };
        check.SetBinding(Button.TextProperty, nameof(TodayHabitItem.CheckText));
        check.SetBinding(VisualElement.BackgroundColorProperty,
            new Binding(nameof(TodayHabitItem.IsDone), converter: new DoneColorConverter()));
        // the command lives in the page view model, the parameter is the row itself
        check.SetBinding(Button.CommandProperty,
            new Binding(nameof(TodayViewModel.CompleteCommand), source: BindingContext));
        check.SetBinding(Button.CommandParameterProperty, Binding.SelfPath);

        var checkBox = new Grid { VerticalOptions = LayoutOptions.Center };
        checkBox.Add(new BoxView { Color = Palette.Ink, Margin = new Thickness(3, 3, 0, 0) });
        checkBox.Add(check);

        var row = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 12
        };
        row.Add(icon, 0);
        row.Add(texts, 1);
        row.Add(checkBox, 2);

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

        if (BindingContext is TodayViewModel vm)
            vm.LoadCommand.Execute(null);
    }

    // Checkbox color: pink when the habit is done, white otherwise.
    private sealed class DoneColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture) =>
            value is true ? Pink : Palette.Card;

        public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture) =>
            throw new NotSupportedException();
    }
}