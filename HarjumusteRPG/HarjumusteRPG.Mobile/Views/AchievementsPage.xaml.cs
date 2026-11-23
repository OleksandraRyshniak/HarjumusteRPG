using HarjumusteRPG.Mobile.ViewModels;

namespace HarjumusteRPG.Mobile.Views;

public partial class AchievementsPage : ContentPage
{
    private readonly VerticalStackLayout _column;

    public AchievementsPage(AchievementsViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
        BackgroundColor = Palette.Bg;
        Shell.SetNavBarIsVisible(this, false);
        this.SetBinding(TitleProperty, nameof(AchievementsViewModel.Title));

        var back = Pixel.LinkButton("< Tagasi", nameof(AchievementsViewModel.BackCommand));
        back.HorizontalOptions = LayoutOptions.Start;
        back.Padding = new Thickness(0);

        var heading = Pixel.Heading("SAAVUTUSED", 16);

        var summary = Pixel.Body(string.Empty, 18, bold: true);
        summary.SetBinding(Label.TextProperty, nameof(AchievementsViewModel.Summary));

        var error = Pixel.ErrorLabel(nameof(AchievementsViewModel.ErrorMessage), nameof(AchievementsViewModel.HasError));

        var busy = new ActivityIndicator { Color = Palette.Ink };
        busy.SetBinding(ActivityIndicator.IsRunningProperty, nameof(AchievementsViewModel.IsBusy));
        busy.SetBinding(IsVisibleProperty, nameof(AchievementsViewModel.IsBusy));

        var list = new VerticalStackLayout { Spacing = 12 };
        BindableLayout.SetItemsSource(list, viewModel.Rows);
        BindableLayout.SetItemTemplate(list, new DataTemplate(CreateRow));

        _column = new VerticalStackLayout
        {
            Padding = new Thickness(20, 16),
            Spacing = 14,
            HorizontalOptions = LayoutOptions.Center,
            Children = { back, heading, summary, error, busy, list }
        };

        Content = new ScrollView { Content = _column };
    }

    // One achievement card: star, name, description and status. Locked ones are faded.
    private static View CreateRow()
    {
        var star = new Image
        {
            Source = "ic_taht.png",
            WidthRequest = 36,
            HeightRequest = 36,
            Aspect = Aspect.AspectFit,
            VerticalOptions = LayoutOptions.Center
        };

        var name = Pixel.Body(string.Empty, 18, bold: true);
        name.SetBinding(Label.TextProperty, nameof(AchievementRow.Name));

        var description = Pixel.Body(string.Empty, 14, color: Palette.Sub);
        description.SetBinding(Label.TextProperty, nameof(AchievementRow.Description));

        var status = Pixel.Body(string.Empty, 13, bold: true, color: Palette.Sub);
        status.SetBinding(Label.TextProperty, nameof(AchievementRow.Status));

        var texts = new VerticalStackLayout
        {
            Spacing = 2,
            VerticalOptions = LayoutOptions.Center,
            Children = { name, description, status }
        };

        var row = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) },
            ColumnSpacing = 12
        };
        row.Add(star, 0);
        row.Add(texts, 1);

        var card = Pixel.Card(row, padding: new Thickness(12));
        card.SetBinding(OpacityProperty, nameof(AchievementRow.Opacity));
        return card;
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

        if (BindingContext is AchievementsViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}