using HarjumusteRPG.Mobile.ViewModels;

namespace HarjumusteRPG.Mobile.Views;

public partial class HabitsPage : ContentPage
{
    private readonly VerticalStackLayout _column;

    public HabitsPage(HabitsViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
        BackgroundColor = Palette.Bg;
        Shell.SetNavBarIsVisible(this, false);
        this.SetBinding(TitleProperty, nameof(HabitsViewModel.Title));

        // --- top: back link and title ---
        var back = Pixel.LinkButton(AppResources.BackToday, nameof(HabitsViewModel.BackCommand));
        back.HorizontalOptions = LayoutOptions.Start;
        back.Padding = new Thickness(0);

        var heading = Pixel.Heading(AppResources.HabitsTitle, 16);

        // --- search ---
        var search = Pixel.Field(AppResources.SearchHabit, nameof(HabitsViewModel.Search),
            returnType: ReturnType.Search, returnCommandPath: nameof(HabitsViewModel.RunSearchCommand));

        // --- filter: all / active / paused ---
        var filters = new Grid
        {
            ColumnSpacing = 6,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            }
        };
        filters.Add(Ui.CommandButton(AppResources.FilterAll, nameof(HabitsViewModel.SetFilterCommand), "all",
            nameof(HabitsViewModel.AllSelected)), 0);
        filters.Add(Ui.CommandButton(AppResources.FilterActive, nameof(HabitsViewModel.SetFilterCommand), "active",
            nameof(HabitsViewModel.ActiveSelected)), 1);
        filters.Add(Ui.CommandButton(AppResources.Paused, nameof(HabitsViewModel.SetFilterCommand), "inactive",
            nameof(HabitsViewModel.InactiveSelected)), 2);

        // --- sorting and "new habit" ---
        var sort = Ui.MakeButton(null, Palette.Card, 15);
        sort.SetBinding(Button.TextProperty, nameof(HabitsViewModel.SortText));
        sort.SetBinding(Button.CommandProperty, nameof(HabitsViewModel.ToggleSortCommand));

        var toolbar = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }
        };
        toolbar.Add(Ui.WithShadow(sort), 0);
        toolbar.Add(Ui.CommandButton(AppResources.NewShort, nameof(HabitsViewModel.AddCommand), fill: Palette.Hero), 1);

        // --- messages ---
        var error = Pixel.ErrorLabel(nameof(HabitsViewModel.ErrorMessage), nameof(HabitsViewModel.HasError));

        var busy = new ActivityIndicator { Color = Palette.Ink };
        busy.SetBinding(ActivityIndicator.IsRunningProperty, nameof(HabitsViewModel.IsBusy));
        busy.SetBinding(IsVisibleProperty, nameof(HabitsViewModel.IsBusy));

        var empty = Pixel.Body(AppResources.HabitsNotFound, 17, color: Palette.Sub);
        empty.SetBinding(IsVisibleProperty, nameof(HabitsViewModel.IsEmpty));

        var count = Pixel.Body(string.Empty, 14, color: Palette.Sub);
        count.SetBinding(Label.TextProperty, nameof(HabitsViewModel.CountText));

        // --- list ---
        var list = new VerticalStackLayout { Spacing = 12 };
        BindableLayout.SetItemsSource(list, viewModel.Habits);
        BindableLayout.SetItemTemplate(list, new DataTemplate(CreateRow));

        // --- paging ---
        var pageText = Pixel.Body(string.Empty, 18, bold: true, center: true);
        pageText.VerticalOptions = LayoutOptions.Center;
        pageText.SetBinding(Label.TextProperty, nameof(HabitsViewModel.PageText));

        var prev = Ui.CommandButton("<", nameof(HabitsViewModel.PrevPageCommand));
        prev.WidthRequest = 56;
        var next = Ui.CommandButton(">", nameof(HabitsViewModel.NextPageCommand));
        next.WidthRequest = 56;

        var paging = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            }
        };
        paging.Add(prev, 0);
        paging.Add(pageText, 1);
        paging.Add(next, 2);

        _column = new VerticalStackLayout
        {
            Padding = new Thickness(20, 16),
            Spacing = 12,
            HorizontalOptions = LayoutOptions.Center,
            Children = { back, heading, search, filters, toolbar, error, busy, list, empty, count, paging }
        };

        Content = new ScrollView { Content = _column };
    }

    // One habit card: icon box with the first letter, name, schedule. Tap to edit.
    private View CreateRow()
    {
        var initial = Pixel.Body(string.Empty, 20, bold: true, center: true);
        initial.VerticalOptions = LayoutOptions.Center;
        initial.SetBinding(Label.TextProperty, nameof(HabitRow.Initial));

        var icon = Pixel.Card(initial, Palette.Bg, border: 3, shadow: 3, padding: new Thickness(0));
        icon.WidthRequest = 44;
        icon.HeightRequest = 44;
        icon.VerticalOptions = LayoutOptions.Center;

        var name = Pixel.Body(string.Empty, 19, bold: true);
        name.LineBreakMode = LineBreakMode.WordWrap;
        name.SetBinding(Label.TextProperty, nameof(HabitRow.Name));

        var subtitle = Pixel.Body(string.Empty, 14, color: Palette.Sub);
        subtitle.SetBinding(Label.TextProperty, nameof(HabitRow.Subtitle));

        var texts = new VerticalStackLayout
        {
            Spacing = 2,
            VerticalOptions = LayoutOptions.Center,
            Children = { name, subtitle }
        };

        var row = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) },
            ColumnSpacing = 12
        };
        row.Add(icon, 0);
        row.Add(texts, 1);

        var card = Pixel.Card(row, padding: new Thickness(12));

        var tap = new TapGestureRecognizer();
        tap.SetBinding(TapGestureRecognizer.CommandProperty,
            new Binding(nameof(HabitsViewModel.EditCommand), source: BindingContext));
        tap.SetBinding(TapGestureRecognizer.CommandParameterProperty, Binding.SelfPath);
        card.GestureRecognizers.Add(tap);

        return card;
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (width > 0)
            _column.WidthRequest = Math.Min(width, 420);
    }

    // Also runs when we come back from the edit page, so the list is refreshed.
    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is HabitsViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}