using HarjumusteRPG.Mobile.Services;

namespace HarjumusteRPG.Mobile.Views;

// Page headers: settings button, and on the main pages also coins and level.
public static class Header
{
    private static bool _opening;

    public static Grid SettingsButton(double size = 44)
    {
        var gear = new ImageButton
        {
            Source = "ic_seaded.png",
            WidthRequest = size,
            HeightRequest = size,
            Padding = new Thickness(size / 5.5),
            BackgroundColor = Palette.Card,
            BorderColor = Palette.Ink,
            BorderWidth = 3,
            CornerRadius = 0,
            Margin = new Thickness(0, 0, 3, 3),
            Command = new Command(async () =>
            {
                if (_opening)
                    return;

                _opening = true;
                try
                {
                    await Nav.GoAsync("settings");
                }
                finally
                {
                    // a double tap must not open the page twice
                    await Task.Delay(400);
                    _opening = false;
                }
            })
        };
        SemanticProperties.SetDescription(gear, AppResources.Settings);

        var grid = new Grid { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.End };
        grid.Add(new BoxView { Color = Palette.Ink, Margin = new Thickness(3, 3, 0, 0) });
        grid.Add(gear);
        return grid;
    }

    // Title on the left, settings button on the right (pages opened from other pages).
    public static Grid Row(View title)
    {
        title.VerticalOptions = LayoutOptions.Center;

        var row = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            ColumnSpacing = 8
        };
        row.Add(title, 0);
        row.Add(SettingsButton(), 1);
        return row;
    }

    // Main pages: title on the left; small coins, level and settings on the right.
    public static Grid StatsRow(View title)
    {
        title.VerticalOptions = LayoutOptions.Center;
        if (title is Label label)
            label.FontSize = 13;

        var right = new HorizontalStackLayout
        {
            Spacing = 6,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                Chip(nameof(Player.CoinsText), Palette.Card, "ic_mynt.png"),
                Chip(nameof(Player.LevelText), Palette.Mint, "ic_taht.png"),
                SettingsButton(36)
            }
        };

        var row = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            ColumnSpacing = 8
        };
        row.Add(title, 0);
        row.Add(right, 1);
        return row;
    }

    // Small card with an icon and a value from Player.Current.
    private static Grid Chip(string path, Color fill, string icon)
    {
        var label = Pixel.Body(string.Empty, 15, bold: true, center: true);
        label.VerticalOptions = LayoutOptions.Center;
        label.SetBinding(Label.TextProperty, new Binding(path, source: Player.Current));

        var content = new HorizontalStackLayout
        {
            Spacing = 4,
            Children =
            {
                new Image { Source = icon, WidthRequest = 18, HeightRequest = 18, VerticalOptions = LayoutOptions.Center },
                label
            }
        };
        return Pixel.Card(content, fill, border: 3, shadow: 3, padding: new Thickness(6, 4));
    }
}