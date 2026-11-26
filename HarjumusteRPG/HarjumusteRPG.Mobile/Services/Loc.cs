using System.Globalization;
using System.Resources;

[assembly: NeutralResourcesLanguage("et")]

namespace HarjumusteRPG.Mobile;

// Selected UI language (ET/EN). Texts come from AppResources (.resx files).
public static class Loc
{
    private const string PrefKey = "language";

    public static string Language { get; private set; } = Preferences.Default.Get(PrefKey, "et");

    public static bool IsEnglish => Language == "en";

    public static CultureInfo Culture { get; private set; } = Make(Language);

    public static void SetLanguage(string code)
    {
        Language = code == "en" ? "en" : "et";
        Preferences.Default.Set(PrefKey, Language);
        Culture = Make(Language);
    }

    // Formats a text from AppResources that has {0}, {1}... placeholders.
    public static string F(string format, params object?[] args) =>
        string.Format(Culture, format, args);

    private static CultureInfo Make(string code) => new(code == "en" ? "en-US" : "et-EE");
}