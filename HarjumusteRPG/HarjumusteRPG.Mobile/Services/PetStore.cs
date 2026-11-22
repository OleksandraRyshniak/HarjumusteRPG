namespace HarjumusteRPG.Mobile.Services;

public record PetOption(string Image, string Label);

// The chosen pet is kept on the device, separately for every user name.
public static class PetStore
{
    public static readonly PetOption[] Options =
    {
        new("kass_hall", "Hall kass"),
        new("kutsikas", "Koer")
    };

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
}