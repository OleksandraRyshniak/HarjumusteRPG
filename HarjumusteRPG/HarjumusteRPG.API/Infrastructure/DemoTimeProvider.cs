namespace HarjumusteRPG.API.Infrastructure;

// Clock of the API. In Development the demo endpoint can move it forward by whole days,
// so the streak, the daily gift and the reward limit can be shown without waiting.
public class DemoTimeProvider : TimeProvider
{
    private long _offsetTicks;

    public TimeSpan Offset => TimeSpan.FromTicks(Interlocked.Read(ref _offsetTicks));

    public override DateTimeOffset GetUtcNow() => base.GetUtcNow() + Offset;

    public void AddDays(int days) => Interlocked.Add(ref _offsetTicks, TimeSpan.FromDays(days).Ticks);

    public void Reset() => Interlocked.Exchange(ref _offsetTicks, 0);
}