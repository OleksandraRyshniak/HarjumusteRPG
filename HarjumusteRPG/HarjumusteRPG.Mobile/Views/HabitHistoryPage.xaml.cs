using HarjumusteRPG.Mobile.Models;
using HarjumusteRPG.Mobile.ViewModels;
using Microsoft.Maui.Controls.Shapes;

namespace HarjumusteRPG.Mobile.Views;

public partial class HabitHistoryPage : ContentPage
{
    private readonly VerticalStackLayout _column;

    public HabitHistoryPage(HabitHistoryViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
        BackgroundColor = Palette.Bg;
        Shell.SetNavBarIsVisible(this, false);
        this.SetBinding(TitleProperty, nameof(HabitHistoryViewModel.Title));

        var back = Pixel.LinkButton(AppResources.Back, nameof(HabitHistoryViewModel.BackCommand));
        back.HorizontalOptions = LayoutOptions.Start;
        back.Padding = new Thickness(0);

        var heading = Pixel.Heading(AppResources.HistoryTitle, 16);

        // --- habit name with its icon ---
        var picture = new Image
        {
            WidthRequest = 32,
            HeightRequest = 32,
            VerticalOptions = LayoutOptions.Center
        };
        picture.SetBinding(Image.SourceProperty, nameof(HabitHistoryViewModel.IconFile));
        picture.SetBinding(IsVisibleProperty, nameof(HabitHistoryViewModel.HasIcon));

        var name = Pixel.Body(string.Empty, 22, bold: true);
        name.VerticalOptions = LayoutOptions.Center;
        name.SetBinding(Label.TextProperty, nameof(HabitHistoryViewModel.HabitName));

        var nameRow = new HorizontalStackLayout { Spacing = 10, Children = { picture, name } };

        // --- calendar: weekday letters and 4 weeks of squares ---
        var letters = new FlexLayout { Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap };
        foreach (var day in Weekdays.Order)
        {
            var letter = Pixel.Body(Weekdays.Letter(day), 14, bold: true, color: Palette.Sub, center: true);
            letter.WidthRequest = 38;
            letter.Margin = new Thickness(0, 0, 6, 0);
            letters.Children.Add(letter);
        }

        var calendar = new FlexLayout { Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap };
        BindableLayout.SetItemsSource(calendar, viewModel.Days);
        BindableLayout.SetItemTemplate(calendar, new DataTemplate(CreateDay));

        var legend = Pixel.Body(AppResources.HistoryLegend, 13, color: Palette.Sub);

        var calendarCard = Pixel.Card(new VerticalStackLayout
        {
            Spacing = 8,
            Children = { letters, calendar, legend }
        });

        // --- list of completions ---
        var summary = Pixel.Body(string.Empty, 16, bold: true);
        summary.SetBinding(Label.TextProperty, nameof(HabitHistoryViewModel.Summary));

        var rows = new VerticalStackLayout { Spacing = 8 };
        BindableLayout.SetItemsSource(rows, viewModel.Rows);
        BindableLayout.SetItemTemplate(rows, new DataTemplate(CreateRow));

        var empty = Pixel.Body(AppResources.HistoryEmpty, 17, color: Palette.Sub);
        empty.SetBinding(IsVisibleProperty, nameof(HabitHistoryViewModel.IsEmpty));

        var error = Pixel.ErrorLabel(nameof(HabitHistoryViewModel.ErrorMessage), nameof(HabitHistoryViewModel.HasError));

        var busy = new ActivityIndicator { Color = Palette.Ink };
        busy.SetBinding(ActivityIndicator.IsRunningProperty, nameof(HabitHistoryViewModel.IsBusy));
        busy.SetBinding(IsVisibleProperty, nameof(HabitHistoryViewModel.IsBusy));

        _column = new VerticalStackLayout
        {
            Padding = new Thickness(20, 16),
            Spacing = 14,
            HorizontalOptions = LayoutOptions.Center,
            Children = { back, heading, nameRow, error, busy, calendarCard, summary, rows, empty }
        };

        Content = new ScrollView { Content = _column };
    }

    // One square of the calendar: pink = done, lavender = today, faded = future.
    private static View CreateDay()
    {
        var number = Pixel.Body(string.Empty, 14, bold: true, center: true);
        number.VerticalOptions = LayoutOptions.Center;
        number.SetBinding(Label.TextProperty, nameof(CalendarDay.Number));

        var cell = new Border
        {
            WidthRequest = 38,
            HeightRequest = 38,
            Stroke = Palette.Ink,
            StrokeThickness = 2,
            StrokeShape = new Rectangle(),
            Margin = new Thickness(0, 0, 6, 6),
            Content = number
        };
        cell.SetBinding(VisualElement.BackgroundColorProperty,
            new Binding(nameof(CalendarDay.State), converter: new StateColorConverter()));
        cell.SetBinding(VisualElement.OpacityProperty, nameof(CalendarDay.Opacity));
        return cell;
    }

    // Date on the left, earned XP on the right.
    private static View CreateRow()
    {
        var date = Pixel.Body(string.Empty, 16);
        date.VerticalOptions = LayoutOptions.Center;
        date.SetBinding(Label.TextProperty, nameof(HistoryRow.DateText));

        var xp = Pixel.Body(string.Empty, 16, bold: true);
        xp.VerticalOptions = LayoutOptions.Center;
        xp.SetBinding(Label.TextProperty, nameof(HistoryRow.XpText));

        var row = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }
        };
        row.Add(date, 0);
        row.Add(xp, 1);

        return Pixel.Card(row, border: 3, shadow: 3, padding: new Thickness(12, 8));
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (width > 0)
            _column.WidthRequest = Math.Min(width, 420);
    }

    private sealed class StateColorConverter : IValueConverter
    {
        private static Color Done => Palette.Pink;

        public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture) =>
            value switch
            {
                DayState.Done => Done,
                DayState.Today => Palette.Hero,
                _ => Palette.Card
            };

        public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture) =>
            throw new NotSupportedException();
    }
}