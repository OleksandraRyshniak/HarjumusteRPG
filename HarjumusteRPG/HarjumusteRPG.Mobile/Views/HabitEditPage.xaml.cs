using HarjumusteRPG.Mobile.ViewModels;

namespace HarjumusteRPG.Mobile.Views;

public partial class HabitEditPage : ContentPage
{
    private readonly VerticalStackLayout _column;

    public HabitEditPage(HabitEditViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
        BackgroundColor = Palette.Bg;
        Shell.SetNavBarIsVisible(this, false);
        this.SetBinding(TitleProperty, nameof(HabitEditViewModel.Title));

        var heading = Pixel.Heading(string.Empty, 16);
        heading.SetBinding(Label.TextProperty, nameof(HabitEditViewModel.Title));

        // --- weekday buttons, shown only when the habit is not daily ---
        var days = new FlexLayout { Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap };
        BindableLayout.SetItemsSource(days, viewModel.Days);
        BindableLayout.SetItemTemplate(days, new DataTemplate(CreateDay));

        var daysBox = new VerticalStackLayout
        {
            Spacing = 8,
            Children = { Pixel.Body(AppResources.Weekdays, 16, bold: true), days }
        };
        daysBox.SetBinding(IsVisibleProperty, nameof(HabitEditViewModel.ShowDays));

        // --- icon picker ---
        var icons = new FlexLayout { Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap };
        BindableLayout.SetItemsSource(icons, viewModel.Icons);
        BindableLayout.SetItemTemplate(icons, new DataTemplate(CreateIcon));

        var iconsBox = new VerticalStackLayout
        {
            Spacing = 8,
            Children = { Pixel.Body(AppResources.Icon, 16, bold: true), icons }
        };

        var activeRow = SwitchRow(AppResources.ActiveLabel, nameof(HabitEditViewModel.IsActive));
        activeRow.SetBinding(IsVisibleProperty, nameof(HabitEditViewModel.IsEditing));

        var error = Pixel.ErrorLabel(nameof(HabitEditViewModel.ErrorMessage), nameof(HabitEditViewModel.HasError));

        var busy = new ActivityIndicator { Color = Palette.Ink };
        busy.SetBinding(ActivityIndicator.IsRunningProperty, nameof(HabitEditViewModel.IsBusy));
        busy.SetBinding(IsVisibleProperty, nameof(HabitEditViewModel.IsBusy));

        var form = new VerticalStackLayout
        {
            Spacing = 14,
            Children =
            {
                Pixel.Field(AppResources.HabitName, nameof(HabitEditViewModel.Name)),
               
                iconsBox,
                SwitchRow(AppResources.EveryDay, nameof(HabitEditViewModel.IsDaily)),
                daysBox,
                activeRow,
                error,
                busy
            }
        };

        // --- buttons ---
        var delete = Ui.MakeButton(null, Palette.Card);
        delete.TextColor = Palette.Err;
        delete.SetBinding(Button.TextProperty, nameof(HabitEditViewModel.DeleteText));
        delete.SetBinding(Button.CommandProperty, nameof(HabitEditViewModel.DeleteCommand));
        var deleteBox = Ui.WithShadow(delete);
        var history = Ui.CommandButton(AppResources.History, nameof(HabitEditViewModel.OpenHistoryCommand));
        history.SetBinding(IsVisibleProperty, nameof(HabitEditViewModel.IsEditing));
        deleteBox.SetBinding(IsVisibleProperty, nameof(HabitEditViewModel.IsEditing));

        _column = new VerticalStackLayout
        {
            Padding = new Thickness(24, 24),
            Spacing = 16,
            HorizontalOptions = LayoutOptions.Center,
            Children =
            {
                heading,
                Pixel.Card(form),
                Pixel.PrimaryButton(AppResources.Save, nameof(HabitEditViewModel.SaveCommand),
                    enabledPath: nameof(HabitEditViewModel.IsNotBusy)),
                history,
                deleteBox,
                Pixel.LinkButton(AppResources.Cancel, nameof(HabitEditViewModel.CancelCommand))
            }
        };

        Content = new ScrollView { Content = _column };
    }

    // Text on the left and a switch on the right.
    private static Grid SwitchRow(string text, string togglePath)
    {
        var label = Pixel.Body(text, 18, bold: true);
        label.VerticalOptions = LayoutOptions.Center;

        var toggle = new Switch
        {
            OnColor = Palette.Hero,
            ThumbColor = Palette.Ink,
            VerticalOptions = LayoutOptions.Center
        };
        toggle.SetBinding(Switch.IsToggledProperty, togglePath);

        var row = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }
        };
        row.Add(label, 0);
        row.Add(toggle, 1);
        return row;
    }

    // One weekday button; the pink color means "selected".
    private static View CreateDay()
    {
        var button = Ui.MakeButton(null, Palette.Card);
        button.WidthRequest = 38;
        button.HeightRequest = 38;
        button.MinimumHeightRequest = 38;
        button.Padding = new Thickness(0);
        button.SetBinding(Button.TextProperty, nameof(WeekdayItem.Letter));
        button.SetBinding(Button.CommandProperty, nameof(WeekdayItem.ToggleCommand));
        button.SetBinding(VisualElement.BackgroundColorProperty,
            new Binding(nameof(WeekdayItem.IsSelected), converter: new BoolToColorConverter(Palette.Hero, Palette.Card)));

        var box = Ui.WithShadow(button);
        box.Margin = new Thickness(0, 0, 6, 6);
        return box;
    }

    // One icon of the picker; the pink color means "selected".
    private static View CreateIcon()
    {
        var picture = new Image
        {
            WidthRequest = 28,
            HeightRequest = 28,
            Aspect = Aspect.AspectFit,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        picture.SetBinding(Image.SourceProperty, nameof(IconChoice.ImageFile));

        var card = Pixel.Card(picture, Palette.Card, border: 3, shadow: 3, padding: new Thickness(0));
        card.WidthRequest = 48;
        card.HeightRequest = 48;
        card.Margin = new Thickness(0, 0, 8, 8);

        // the second child of the card is the bordered face
        var face = (Border)card.Children[1];
        face.SetBinding(VisualElement.BackgroundColorProperty,
            new Binding(nameof(IconChoice.IsSelected), converter: new BoolToColorConverter(Palette.Hero, Palette.Card)));

        var tap = new TapGestureRecognizer();
        tap.SetBinding(TapGestureRecognizer.CommandProperty, nameof(IconChoice.PickCommand));
        card.GestureRecognizers.Add(tap);
        return card;
    }
    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (width > 0)
            _column.WidthRequest = Math.Min(width, 420);
    }
}