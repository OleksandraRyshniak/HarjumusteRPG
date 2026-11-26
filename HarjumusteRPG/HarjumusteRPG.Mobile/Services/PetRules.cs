namespace HarjumusteRPG.Mobile.Services;

public enum PetAction { Pat, Play, Feed, Habit }

public enum PetState { Happy, Calm, Sad, Hungry, Tired }

// How the three needs (joy, food, energy) change and depend on each other.
public static class PetRules
{
    public const int FeedCost = 5;
    public const int Low = 20;
    private const int MaxHours = 72;

    // Time passes: food goes down; a fed pet rests, a hungry pet loses energy and joy.
    public static PetMood PassHours(PetMood m, int hours)
    {
        for (var i = 0; i < Math.Min(hours, MaxHours); i++)
        {
            var hungry = m.Food < Low;
            m = Clamp(new PetMood(
                Joy: m.Joy - 1 - (hungry ? 3 : 0),
                Food: m.Food - 3,
                Energy: m.Energy + (hungry ? -2 : 2)));
        }
        return m;
    }

    // Why the action can't be done now (text with {0} = pet name), or null if it can.
    public static string? Blocker(PetMood m, PetAction action) => action switch
    {
        PetAction.Play when m.Food < Low => AppResources.PetTooHungryMessage,
        PetAction.Play when m.Energy < Low => AppResources.PetTiredMessage,
        PetAction.Feed when m.Food >= 90 => AppResources.PetFullMessage,
        _ => null
    };

    public static PetMood Apply(PetMood m, PetAction action) => Clamp(action switch
    {
        PetAction.Pat => m with { Joy = m.Joy + 8 },
        PetAction.Play => new PetMood(m.Joy + 15, m.Food - 10, m.Energy - 20),
        PetAction.Feed => new PetMood(m.Joy + 3, m.Food + 30, m.Energy + (m.Food < Low ? 20 : 10)),
        PetAction.Habit => m with { Joy = m.Joy + 10, Food = m.Food + 10 },
        _ => m
    });

    public static PetState State(PetMood m)
    {
        if (m.Food < Low) return PetState.Hungry;
        if (m.Energy < Low) return PetState.Tired;
        if (m.Joy < 40) return PetState.Sad;
        if (m.Joy >= 70 && m.Food >= 50 && m.Energy >= 50) return PetState.Happy;
        return PetState.Calm;
    }

    private static PetMood Clamp(PetMood m) =>
        new(Math.Clamp(m.Joy, 0, 100), Math.Clamp(m.Food, 0, 100), Math.Clamp(m.Energy, 0, 100));
}