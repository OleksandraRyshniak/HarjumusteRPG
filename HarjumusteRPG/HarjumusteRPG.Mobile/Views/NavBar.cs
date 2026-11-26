using Microsoft.Maui.Controls.Shapes;
using HarjumusteRPG.Mobile.Services;

namespace HarjumusteRPG.Mobile.Views;

// Bottom menu shared by the main screens. A tab without a route does nothing yet.
public static class NavBar
{
    private record Tab(string Text, string Icon, string? Route);

    // A property (not a static field), so the texts follow the selected language.
    private static Tab[] Tabs => new Tab[]
    {
        new(AppResources.NavToday, "ic_kalender.png", "//main"),
        new(AppResources.NavPet, "ic_suda.png", "//progress"),
        new(AppResources.NavStats, "ic_statistika.png", "//statistics"),
        new(AppResources.NavLeaderboard, "ic_karikas.png", "//leaderboard"),
        new(AppResources.NavShop, "ic_pood.png", "//shop")
    };

    private static bool _navigating;

    public static View Build(int active)
    {
        var tabs = Tabs;
        var inactive = Palette.NavMuted;

        var grid = new Grid { Padding = new Thickness(6, 10) };
        for (var i = 0; i < tabs.Length; i++)
        {
            var tab = tabs[i];
            var isActive = i == active;

            var icon = new Image
            {
                Source = tab.Icon,
                WidthRequest = 28,
                HeightRequest = 28,
                HorizontalOptions = LayoutOptions.Center,
                Opacity = isActive ? 1 : 0.55
            };
            var text = Pixel.Body(tab.Text, 12, bold: true, color: isActive ? Palette.NavText : inactive, center: true);
            var item = new VerticalStackLayout { Spacing = 4, Children = { icon, text } };

            var cell = new Grid();
            cell.Add(item);

            if (tab.Route is not null && !isActive)
            {
                var route = tab.Route;
                // A transparent button on top catches the tap on every platform.
                var hit = new Button
                {
                    BackgroundColor = Colors.Transparent,
                    BorderWidth = 0,
                    Command = new Command(async () =>
                    {
                        if (_navigating)
                            return;

                        _navigating = true;
                        try
                        {
                            await Nav.GoAsync(route);
                        }
                        catch (Exception ex)
                        {
                            await Shell.Current.DisplayAlertAsync(AppResources.Error, ex.Message, "OK");
                        }
                        finally
                        {
                            // give Shell time to finish the navigation before the next tap
                            await Task.Delay(400);
                            _navigating = false;
                        }
                    })
                };
                cell.Add(hit);
            }

            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            grid.Add(cell, i, 0);
        }

        return new Border
        {
            BackgroundColor = Palette.Nav,
            Stroke = Palette.Ink,
            StrokeShape = new Rectangle(),
            Margin = new Thickness(12, 4, 12, 12),
            Content = grid
        };
    }
}