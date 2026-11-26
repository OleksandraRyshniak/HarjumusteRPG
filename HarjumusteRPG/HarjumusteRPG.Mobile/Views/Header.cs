using HarjumusteRPG.Mobile.Services;

namespace HarjumusteRPG.Mobile.Views;

// Settings button (gear) in the top right corner of the pages.
public static class Header
{
    private static bool _opening;

    public static Grid SettingsButton()
    {
        var gear = new ImageButton
        {
            Source = "ic_seaded.png",
            WidthRequest = 44,
            HeightRequest = 44,
            Padding = new Thickness(8),
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
                    await OpenSettingsAsync();
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

        var grid = new Grid { VerticalOptions = LayoutOptions.Start, HorizontalOptions = LayoutOptions.End };
        grid.Add(new BoxView { Color = Palette.Ink, Margin = new Thickness(3, 3, 0, 0) });
        grid.Add(gear);
        return grid;
    }

    // Title on the left, settings button on the right.
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

    // Shell throws if another navigation is still running:
    // wait a moment and try once more, then ignore the tap.
    private static async Task OpenSettingsAsync()
    {
        for (var attempt = 0; attempt < 2; attempt++)
        {
            try
            {
                await Nav.GoAsync("settings");
                return;
            }
            catch (InvalidOperationException)
            {
                await Task.Delay(300);
            }
        }
    }
}