namespace HarjumusteRPG.Mobile.Views;

// Colors of the "Kommipood" theme (theme switching comes in #36).
public static class Palette
{
    public static readonly Color Bg = Color.FromArgb("#FCE4EC");
    public static readonly Color Card = Color.FromArgb("#FFFFFF");
    public static readonly Color Ink = Color.FromArgb("#3D2C4F");
    public static readonly Color Sub = Color.FromArgb("#614D79");
    public static readonly Color Hero = Color.FromArgb("#C9B6F2");
    public static readonly Color Err = Color.FromArgb("#B42318");
}

// Small helpers for the pixel style: thick borders and hard shadows.
public static class Pixel
{
    public const string TitleFont = "PressStart";
    public const string TextFont = "Pixelify";

    public static Label Heading(string text, double size = 14, bool center = false) => new()
    {
        Text = text,
        FontFamily = TitleFont,
        FontSize = size,
        TextColor = Palette.Ink,
        HorizontalOptions = center ? LayoutOptions.Center : LayoutOptions.Start,
        HorizontalTextAlignment = center ? TextAlignment.Center : TextAlignment.Start
    };

    public static Label Body(string text, double size = 16, bool bold = false, Color? color = null, bool center = false) => new()
    {
        Text = text,
        FontFamily = TextFont,
        FontSize = size,
        TextColor = color ?? Palette.Ink,
        FontAttributes = bold ? FontAttributes.Bold : FontAttributes.None,
        HorizontalOptions = center ? LayoutOptions.Center : LayoutOptions.Start,
        HorizontalTextAlignment = center ? TextAlignment.Center : TextAlignment.Start
    };

    // Red error text, shown only when the view model has an error.
    public static Label ErrorLabel(string errorPath, string hasErrorPath)
    {
        var label = Body(string.Empty, 16, bold: true, color: Palette.Err);
        label.SetBinding(Label.TextProperty, errorPath);
        label.SetBinding(VisualElement.IsVisibleProperty, hasErrorPath);
        return label;
    }

    // A box with a thick border and a hard shadow.
    public static Grid Card(View content, Color? fill = null, double border = 4, double shadow = 4, Thickness? padding = null)
    {
        var face = new Border
        {
            Stroke = Palette.Ink,
            StrokeThickness = border,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.Rectangle(),
            BackgroundColor = fill ?? Palette.Card,
            Padding = padding ?? new Thickness(14),
            Margin = new Thickness(0, 0, shadow, shadow),
            Content = content
        };
        var shade = new BoxView { Color = Palette.Ink, Margin = new Thickness(shadow, shadow, 0, 0) };

        var grid = new Grid();
        grid.Add(shade);
        grid.Add(face);
        return grid;
    }

    public static Grid Field(string placeholder, string textPath, bool password = false,
        Keyboard? keyboard = null, ReturnType returnType = ReturnType.Next, string? returnCommandPath = null)
    {
        var entry = new Entry
        {
            Placeholder = placeholder,
            PlaceholderColor = Palette.Sub,
            TextColor = Palette.Ink,
            FontFamily = TextFont,
            FontSize = 18,
            IsPassword = password,
            Keyboard = keyboard ?? Keyboard.Default,
            ReturnType = returnType,
            BackgroundColor = Colors.Transparent
        };
        entry.SetBinding(Entry.TextProperty, textPath);
        if (returnCommandPath is not null)
            entry.SetBinding(Entry.ReturnCommandProperty, returnCommandPath);

        return Card(entry, Palette.Card, border: 3, shadow: 3, padding: new Thickness(10, 0));
    }

    public static Grid PrimaryButton(string text, string commandPath, bool primary = true, string? enabledPath = null)
    {
        var button = new Button
        {
            Text = text,
            FontFamily = TextFont,
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Palette.Ink,
            BackgroundColor = primary ? Palette.Hero : Palette.Card,
            BorderColor = Palette.Ink,
            BorderWidth = 3,
            CornerRadius = 0,
            MinimumHeightRequest = 48,
            Margin = new Thickness(0, 0, 3, 3)
        };
        button.SetBinding(Button.CommandProperty, commandPath);
        if (enabledPath is not null)
            button.SetBinding(VisualElement.IsEnabledProperty, enabledPath);

        var shade = new BoxView { Color = Palette.Ink, Margin = new Thickness(3, 3, 0, 0) };

        var grid = new Grid();
        grid.Add(shade);
        grid.Add(button);
        return grid;
    }

    public static Button LinkButton(string text, string commandPath)
    {
        var button = new Button
        {
            Text = text,
            FontFamily = TextFont,
            FontSize = 16,
            TextColor = Palette.Ink,
            BackgroundColor = Colors.Transparent,
            BorderWidth = 0
        };
        button.SetBinding(Button.CommandProperty, commandPath);
        return button;
    }
}