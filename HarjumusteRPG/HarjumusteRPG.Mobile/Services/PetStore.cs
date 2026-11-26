namespace HarjumusteRPG.Mobile.Services;

public record PetOption(string Image, string Label);

// Pet needs, each 0..100.
public record PetMood(int Joy, int Food, int Energy);

// The chosen pet is kept on the device, separately for every user name.
public static class PetStore
{
    public static readonly PetOption[] Options =
    {
        new("kass_hall", "Hall kass"),
        new("kutsikas", "Koer")
    };

    // Needs are kept on the device; PetRules says how they change over time.
    private const int StartMood = 70;

    // Current needs: the saved values with the hours passed since the last update.
    public static PetMood Mood(string? user)
    {
        var last = Preferences.Default.Get(Key(user, "mood_time"), DateTime.UtcNow);
        var saved = new PetMood(
            Preferences.Default.Get(Key(user, "joy"), StartMood),
            Preferences.Default.Get(Key(user, "food"), StartMood),
            Preferences.Default.Get(Key(user, "energy"), StartMood));

        var hours = (int)Math.Max((DateTime.UtcNow - last).TotalHours, 0);
        if (hours == 0)
            return saved;

        var now = PetRules.PassHours(saved, hours);
        SaveMood(user, now, last.AddHours(hours));
        return now;
    }

    // Does an action (pat, play, feed, habit) and saves the result.
    public static PetMood Apply(string? user, PetAction action)
    {
        var next = PetRules.Apply(Mood(user), action);
        SaveMood(user, next, Preferences.Default.Get(Key(user, "mood_time"), DateTime.UtcNow));
        return next;
    }

    private static void SaveMood(string? user, PetMood mood, DateTime time)
    {
        Preferences.Default.Set(Key(user, "joy"), mood.Joy);
        Preferences.Default.Set(Key(user, "food"), mood.Food);
        Preferences.Default.Set(Key(user, "energy"), mood.Energy);
        Preferences.Default.Set(Key(user, "mood_time"), time);
    }

    private static string Key(string? user, string field) =>
        $"pet_{(user ?? string.Empty).Trim().ToLowerInvariant()}_{field}";

    public static bool HasPet(string? user) => Preferences.Default.ContainsKey(Key(user, "image"));

    // File name of the pet picture, e.g. "kass_hall.png".
    public static string Image(string? user) =>
        Preferences.Default.Get(Key(user, "image"), "kass_hall") + ".png";

    public static string Name(string? user) => Preferences.Default.Get(Key(user, "name"), "Muri");

    public static void Save(string? user, string image, string name)
    {
        Preferences.Default.Set(Key(user, "image"), image);
        Preferences.Default.Set(Key(user, "name"), name);
    }

    // Where to go after login: the main screen, or pet selection if there is no pet yet.
    public static string StartRoute(string? user) => HasPet(user) ? "//main" : "//pet";

    // Names of all pets in the shop (ET / EN).
    private static readonly Dictionary<string, (string Et, string En)> Names = new()
    {
        ["kass_hall"] = ("Hall kass", "Grey cat"),
        ["kutsikas"] = ("Koer", "Dog"),
        ["hamster_hele"] = ("Hamster", "Hamster"),
        ["hiir"] = ("Hiir", "Mouse"),
        ["merisiga"] = ("Merisiga", "Guinea pig"),
        ["kass_pruun"] = ("Pruun kass", "Brown cat"),
        ["janes_valge"] = ("Valge jänes", "White bunny"),
        ["janes_luppkorv"] = ("Luppkõrv", "Lop bunny"),
        ["kass_punane"] = ("Punane kass", "Ginger cat"),
        ["tsintsilja"] = ("Tšintšilja", "Chinchilla")
    };

    public static string Label(string key) =>
        Names.TryGetValue(key, out var n) ? (Loc.IsEnglish ? n.En : n.Et) : key;

    // Changes only the picture of the pet, the name stays.
    public static void SetImage(string? user, string image) =>
        Preferences.Default.Set(Key(user, "image"), image);

    // Demo: the needs change as if this many hours had passed.
    public static void SkipHours(string? user, int hours)
    {
        var last = Preferences.Default.Get(Key(user, "mood_time"), DateTime.UtcNow);
        Preferences.Default.Set(Key(user, "mood_time"), last.AddHours(-hours));
    }
}