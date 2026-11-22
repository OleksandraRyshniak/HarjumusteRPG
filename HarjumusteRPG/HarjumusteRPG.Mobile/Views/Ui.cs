namespace HarjumusteRPG.Mobile.Views;

// Small helpers for buttons in the pixel style.
public static class Ui
{
    public static Button MakeButton(string? text, Color fill, double fontSize = 16) => new()
    {
        Text = text,
        FontFamily = Pixel.TextFont,
        FontSize = fontSize,
        FontAttributes = FontAttributes.Bold,
        TextColor = Palette.Ink,
        BackgroundColor = fill,
        BorderColor = Palette.Ink,
        BorderWidth = 3,
        CornerRadius = 0,
        MinimumHeightRequest = 44,
        Padding = new Thickness(8, 0),
        Margin = new Thickness(0, 0, 3, 3)
    };

    // Adds the hard shadow behind a button.
    public static Grid WithShadow(Button button)
    {
        var grid = new Grid();
        grid.Add(new BoxView { Color = Palette.Ink, Margin = new Thickness(3, 3, 0, 0) });
        grid.Add(button);
        return grid;
    }

    // Button bound to a view model command. selectedPath (a bool property) colors it
    // like a selected tab.
    public static Grid CommandButton(string text, string commandPath, object? parameter = null,
        string? selectedPath = null, Color? fill = null)
    {
        var button = MakeButton(text, fill ?? Palette.Card);
        button.SetBinding(Button.CommandProperty, commandPath);
        if (parameter is not null)
            button.CommandParameter = parameter;
        if (selectedPath is not null)
            button.SetBinding(VisualElement.BackgroundColorProperty,
                new Binding(selectedPath, converter: new BoolToColorConverter(Palette.Hero, Palette.Card)));

        return WithShadow(button);
    }
}

public sealed class BoolToColorConverter : IValueConverter
{
    private readonly Color _on;
    private readonly Color _off;

    public BoolToColorConverter(Color on, Color off)
    {
        _on = on;
        _off = off;
    }

    public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture) =>
        value is true ? _on : _off;

    public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture) =>
        throw new NotSupportedException();
}