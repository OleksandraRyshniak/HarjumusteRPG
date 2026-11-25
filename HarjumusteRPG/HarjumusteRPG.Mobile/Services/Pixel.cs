namespace HarjumusteRPG.Mobile.Views;

// One color theme of the app.
public sealed record Theme(
    string Name,
    Color Bg, Color Card, Color Ink, Color Sub, Color Hero, Color Err,
    Color Nav, Color NavText, Color NavMuted,
    Color Mint, Color Pink, Color Gold);

// Colors of the current theme. Properties read the selected theme every time,
// so pages built after a theme change get the new colors.
public static class Palette
{
    public static readonly Theme[] Themes =
    {
        new("candy",
            Bg: Color.FromArgb("#FCE4EC"), Card: Color.FromArgb("#FFFFFF"), Ink: Color.FromArgb("#3D2C4F"),
            Sub: Color.FromArgb("#614D79"), Hero: Color.FromArgb("#C9B6F2"), Err: Color.FromArgb("#B42318"),
            Nav: Color.FromArgb("#3D2C4F"), NavText: Color.FromArgb("#FFFFFF"), NavMuted: Color.FromArgb("#B9A8D6"),
            Mint: Color.FromArgb("#A8E6CF"), Pink: Color.FromArgb("#F27FA5"), Gold: Color.FromArgb("#FFD966")),
        new("night",
            Bg: Color.FromArgb("#231A33"), Card: Color.FromArgb("#35284D"), Ink: Color.FromArgb("#F4EAFE"),
            Sub: Color.FromArgb("#C4B2E0"), Hero: Color.FromArgb("#7B5CC4"), Err: Color.FromArgb("#FF8A80"),
            Nav: Color.FromArgb("#120C1E"), NavText: Color.FromArgb("#FFFFFF"), NavMuted: Color.FromArgb("#8E7BB0"),
            Mint: Color.FromArgb("#2F7D62"), Pink: Color.FromArgb("#C2477A"), Gold: Color.FromArgb("#B8902B"))
    };

    private const string PrefKey = "theme";

    private static Theme _current = Find(Preferences.Default.Get(PrefKey, "candy"));

    private static Theme Find(string name) =>
        Themes.FirstOrDefault(t => t.Name == name) ?? Themes[0];

    public static string ThemeName => _current.Name;

    public static void SetTheme(string name)
    {
        _current = Find(name);
        Preferences.Default.Set(PrefKey, _current.Name);
    }

    public static Color Bg => _current.Bg;
    public static Color Card => _current.Card;
    public static Color Ink => _current.Ink;
    public static Color Sub => _current.Sub;
    public static Color Hero => _current.Hero;
    public static Color Err => _current.Err;
    public static Color Nav => _current.Nav;
    public static Color NavText => _current.NavText;
    public static Color NavMuted => _current.NavMuted;
    public static Color Mint => _current.Mint;
    public static Color Pink => _current.Pink;
    public static Color Gold => _current.Gold;
}

// Small helpers for the pixel style: thick borders and hard shadows.
public static class Pixel
{
    public const string TitleFont = "PressStart";
    public const string TextFont = "Pixelify";

    public static Label Heading(string text, double size = 16, bool center = false) => new()
    {
        Text = text,
        FontFamily = TitleFont,
        FontSize = size,
        TextColor = Palette.Ink,
        HorizontalOptions = center ? LayoutOptions.Center : LayoutOptions.Start,
        HorizontalTextAlignment = center ? TextAlignment.Center : TextAlignment.Start
    };

    public static Label Body(string text, double size = 18, bool bold = false, Color? color = null, bool center = false) => new()
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