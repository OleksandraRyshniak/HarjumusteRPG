using HarjumusteRPG.Mobile.ViewModels;
using Microsoft.Maui.Controls.Shapes;

namespace HarjumusteRPG.Mobile.Views;

public partial class StatisticsPage : ContentPage
{
    private static readonly Color Pink = Color.FromArgb("#F27FA5");

    private readonly Grid _root;

    public StatisticsPage(StatisticsViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
        BackgroundColor = Palette.Bg;
        Shell.SetNavBarIsVisible(this, false);
        this.SetBinding(TitleProperty, nameof(StatisticsViewModel.Title));

        var heading = Pixel.Heading("STATISTIKA", 16);

        // --- two tabs: week / month ---
        var tabs = new Grid
        {
            ColumnSpacing = 10,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) }
        };
        tabs.Add(Ui.CommandButton("Nädal", nameof(StatisticsViewModel.ShowWeekTabCommand),
            selectedPath: nameof(StatisticsViewModel.ShowWeek)), 0);
        tabs.Add(Ui.CommandButton("Kuu", nameof(StatisticsViewModel.ShowMonthTabCommand),
            selectedPath: nameof(StatisticsViewModel.ShowMonth)), 1);

        var error = Pixel.ErrorLabel(nameof(StatisticsViewModel.ErrorMessage), nameof(StatisticsViewModel.HasError));

        var busy = new ActivityIndicator { Color = Palette.Ink };
        busy.SetBinding(ActivityIndicator.IsRunningProperty, nameof(StatisticsViewModel.IsBusy));
        busy.SetBinding(IsVisibleProperty, nameof(StatisticsViewModel.IsBusy));

        // --- chart ---
        var chartTitle = Pixel.Heading(string.Empty, 12);
        chartTitle.SetBinding(Label.TextProperty, nameof(StatisticsViewModel.ChartTitle));

        var range = Pixel.Body(string.Empty, 14, color: Palette.Sub);
        range.SetBinding(Label.TextProperty, nameof(StatisticsViewModel.RangeText));

        // bars stand on one line: every column is aligned to the bottom
        var bars = new HorizontalStackLayout
        {
            Spacing = 4,
            HeightRequest = 180,
            HorizontalOptions = LayoutOptions.Center
        };
        BindableLayout.SetItemsSource(bars, viewModel.Bars);
        BindableLayout.SetItemTemplate(bars, new DataTemplate(CreateBar));

        var empty = Pixel.Body("Sellel perioodil pole veel täitmisi.", 15, color: Palette.Sub);
        empty.SetBinding(IsVisibleProperty, nameof(StatisticsViewModel.IsEmpty));

        var chartCard = Pixel.Card(new VerticalStackLayout
        {
            Spacing = 8,
            Children = { chartTitle, range, bars, empty }
        });

        // --- four numbers ---
        var stats = new Grid
        {
            ColumnSpacing = 12,
            RowSpacing = 12,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) },
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto) }
        };
        stats.Add(Tile("ic_kalender.png", nameof(StatisticsViewModel.TotalText), "täitmist"), 0, 0);
        stats.Add(Tile("ic_valk.png", nameof(StatisticsViewModel.XpText), "XP kokku"), 1, 0);
        stats.Add(Tile("ic_tuli.png", nameof(StatisticsViewModel.DaysText), "aktiivset päeva"), 0, 1);
        stats.Add(Tile("ic_taht.png", nameof(StatisticsViewModel.BestText), "parim päev"), 1, 1);

        var column = new VerticalStackLayout
        {
            Padding = new Thickness(20, 20, 20, 12),
            Spacing = 14,
            Children = { heading, tabs, error, busy, chartCard, stats }
        };

        _root = new Grid
        {
            RowDefinitions = { new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto) },
            HorizontalOptions = LayoutOptions.Center
        };
        _root.Add(new ScrollView { Content = column }, 0, 0);
        _root.Add(NavBar.Build(2), 0, 1);

        Content = _root;
    }

    // One column of the chart: number, bar and weekday letter. Today's bar is pink.
    private static View CreateBar()
    {
        var value = Pixel.Body(string.Empty, 12, bold: true, center: true);
        value.SetBinding(Label.TextProperty, nameof(BarItem.ValueText));

        var bar = new Border
        {
            Stroke = Palette.Ink,
            StrokeThickness = 2,
            StrokeShape = new Rectangle(),
            HorizontalOptions = LayoutOptions.Center
        };
        bar.SetBinding(VisualElement.WidthRequestProperty, nameof(BarItem.Width));
        bar.SetBinding(VisualElement.HeightRequestProperty, nameof(BarItem.Height));
        bar.SetBinding(VisualElement.BackgroundColorProperty,
            new Binding(nameof(BarItem.IsToday), converter: new BoolToColorConverter(Pink, Palette.Hero)));

        var label = Pixel.Body(string.Empty, 13, bold: true, color: Palette.Sub, center: true);
        label.SetBinding(Label.TextProperty, nameof(BarItem.Label));

        return new VerticalStackLayout
        {
            Spacing = 2,
            VerticalOptions = LayoutOptions.End,
            Children = { value, bar, label }
        };
    }

    // Icon, number and a short caption.
    private static Grid Tile(string icon, string valuePath, string caption)
    {
        var value = Pixel.Body(string.Empty, 20, bold: true);
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

        if (BindingContext is StatisticsViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}