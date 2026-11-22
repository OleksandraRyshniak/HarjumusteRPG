using HarjumusteRPG.Mobile.ViewModels;
using Microsoft.Maui.Controls.Shapes;

namespace HarjumusteRPG.Mobile.Views;

public partial class TodayPage : ContentPage
{
    private static readonly Color Mint = Color.FromArgb("#A8E6CF");
    private static readonly Color Pink = Color.FromArgb("#F27FA5");

    private readonly Grid _root;

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
                Chip(nameof(TodayViewModel.LevelChip), Mint, "ic_taht.png")
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

        var empty = Pixel.Body("Täna pole ühtegi harjumust. Lisa uus harjumus!", 17, color: Palette.Sub);
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
        sectionRow,
        reward,
        error,
        busy,
        list,
        empty,
        Pixel.PrimaryButton("Halda harjumusi", nameof(TodayViewModel.ManageCommand), primary: false),
        Pixel.PrimaryButton("Logi välja", nameof(TodayViewModel.LogoutCommand), primary: false)
    }
        };

        _root = new Grid
        {
            RowDefinitions = { new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto) },
            HorizontalOptions = LayoutOptions.Center
        };
        _root.Add(new ScrollView { Content = column }, 0, 0);
        _root.Add(BuildNavBar(), 0, 1);

        Content = _root;
    }

    // Small box in the header: coins or level.
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

    // Bottom menu. Only "Täna" works for now; other tabs get their screens later.
    // Bottom menu. Only "Täna" works for now; other tabs get their screens later.
    private static View BuildNavBar()
    {
        var tabs = new (string Text, string Icon)[]
        {
        ("Täna", "ic_kalender.png"),
        ("Lemmik", "ic_suda.png"),
        ("Statistika", "ic_statistika.png"),
        ("Edetabel", "ic_karikas.png"),
        ("Pood", "ic_pood.png")
        };
        var inactive = Color.FromArgb("#B9A8D6");

        var grid = new Grid { Padding = new Thickness(6, 10) };
        for (var i = 0; i < tabs.Length; i++)
        {
            var active = i == 0;
            var icon = new Image
            {
                Source = tabs[i].Icon,
                WidthRequest = 28,
                HeightRequest = 28,
                HorizontalOptions = LayoutOptions.Center,
                Opacity = active ? 1 : 0.55
            };
            var text = Pixel.Body(tabs[i].Text, 12, bold: true, color: active ? Colors.White : inactive, center: true);

            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            grid.Add(new VerticalStackLayout { Spacing = 4, Children = { icon, text } }, i, 0);
        }

        return new Border
        {
            BackgroundColor = Palette.Ink,
            Stroke = Palette.Ink,
            StrokeShape = new Rectangle(),
            Margin = new Thickness(12, 4, 12, 12),
            Content = grid
        };
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