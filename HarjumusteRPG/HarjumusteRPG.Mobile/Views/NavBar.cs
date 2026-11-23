using Microsoft.Maui.Controls.Shapes;

namespace HarjumusteRPG.Mobile.Views;

// Bottom menu shared by the main screens. A tab without a route does nothing yet.
public static class NavBar
{
    private record Tab(string Text, string Icon, string? Route);

    private static readonly Tab[] Tabs =
    {
        new("Täna", "ic_kalender.png", "//main"),
        new("Lemmik", "ic_suda.png", "//progress"),
        new("Statistika", "ic_statistika.png", null),
        new("Edetabel", "ic_karikas.png", null),
        new("Pood", "ic_pood.png", null)
    };

    private static bool _navigating;

    public static View Build(int active)
    {
        var inactive = Color.FromArgb("#B9A8D6");

        var grid = new Grid { Padding = new Thickness(6, 10) };
        for (var i = 0; i < Tabs.Length; i++)
        {
            var tab = Tabs[i];
            var isActive = i == active;

            var icon = new Image
            {
                Source = tab.Icon,
                WidthRequest = 28,
                HeightRequest = 28,
                HorizontalOptions = LayoutOptions.Center,
                Opacity = isActive ? 1 : 0.55
            };
            var text = Pixel.Body(tab.Text, 12, bold: true, color: isActive ? Colors.White : inactive, center: true);
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
                            await Shell.Current.GoToAsync(route);
                        }
                        catch (Exception ex)
                        {
                            await Shell.Current.DisplayAlertAsync("Viga", ex.Message, "OK");
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
            BackgroundColor = Palette.Ink,
            Stroke = Palette.Ink,
            StrokeShape = new Rectangle(),
            Margin = new Thickness(12, 4, 12, 12),
            Content = grid
        };
    }
}