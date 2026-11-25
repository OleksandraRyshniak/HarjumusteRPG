namespace HarjumusteRPG.Mobile.Services;

public record HabitIconOption(string Key, string File, string Label);

// The icon of a habit is kept on the device, separately for every user and habit.
public static class HabitIcons
{
    public static readonly HabitIconOption[] Options =
    {
        new("vesi", "ic_vesi.png", "Vesi"),
        new("sport", "ic_sport.png", "Sport"),
        new("raamat", "ic_raamat.png", "Lugemine"),
        new("uni", "ic_uni.png", "Uni"),
        new("ravim", "ic_ravim.png", "Ravim"),
        new("oun", "ic_oun.png", "Toit"),
        new("noot", "ic_noot.png", "Muusika"),
        new("pliiats", "ic_pliiats.png", "Kirjutamine"),
        new("kell", "ic_kell.png", "Aeg"),
        new("leht", "ic_leht.png", "Loodus"),
        new("suda", "ic_suda.png", "Tervis"),
        new("kauss", "ic_kauss.png", "Lemmik"),
    };

    private static string PrefKey(string? user, int habitId) =>
        $"habiticon_{(user ?? string.Empty).Trim().ToLowerInvariant()}_{habitId}";

    // Key of the chosen icon, or null when the habit has none.
    public static string? GetKey(string? user, int habitId)
    {
        var key = Preferences.Default.Get(PrefKey(user, habitId), string.Empty);
        return key.Length == 0 ? null : key;
    }

    // File name of the icon picture, or null.
    public static string? GetFile(string? user, int habitId)
    {
        var key = GetKey(user, habitId);
        return Options.FirstOrDefault(o => o.Key == key)?.File;
    }

    public static void Set(string? user, int habitId, string? key)
    {
        if (string.IsNullOrEmpty(key))
            Preferences.Default.Remove(PrefKey(user, habitId));
        else
            Preferences.Default.Set(PrefKey(user, habitId), key);
    }
}