namespace HarjumusteRPG.Tests;

// A clock that always shows the same moment (UTC), so the tests do not depend on today's date.
internal sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}