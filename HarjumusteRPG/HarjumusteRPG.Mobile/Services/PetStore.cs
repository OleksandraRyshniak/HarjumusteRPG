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

    // Needs are kept on the device and drop 2 points per hour.
    private const int StartMood = 70;
    private const int DropPerHour = 2;

    public static PetMood Mood(string? user)
    {
        var last = Preferences.Default.Get(Key(user, "mood_time"), DateTime.UtcNow);
        var drop = (int)(Math.Max((DateTime.UtcNow - last).TotalHours, 0) * DropPerHour);

        return new PetMood(
            Clamp(Preferences.Default.Get(Key(user, "joy"), StartMood) - drop),
            Clamp(Preferences.Default.Get(Key(user, "food"), StartMood) - drop),
            Clamp(Preferences.Default.Get(Key(user, "energy"), StartMood) - drop));
    }

    public static PetMood ChangeMood(string? user, int joy = 0, int food = 0, int energy = 0)
    {
        var now = Mood(user);
        var next = new PetMood(Clamp(now.Joy + joy), Clamp(now.Food + food), Clamp(now.Energy + energy));

        Preferences.Default.Set(Key(user, "joy"), next.Joy);
        Preferences.Default.Set(Key(user, "food"), next.Food);
        Preferences.Default.Set(Key(user, "energy"), next.Energy);
        Preferences.Default.Set(Key(user, "mood_time"), DateTime.UtcNow);
        return next;
    }

    private static int Clamp(int value) => Math.Clamp(value, 0, 100);

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

    // Demo: the needs drop as if this many hours had passed.
    public static void SkipHours(string? user, int hours)
    {
        var last = Preferences.Default.Get(Key(user, "mood_time"), DateTime.UtcNow);
        Preferences.Default.Set(Key(user, "mood_time"), last.AddHours(-hours));
    }
}