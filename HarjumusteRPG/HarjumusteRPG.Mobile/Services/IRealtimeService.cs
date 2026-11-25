namespace HarjumusteRPG.Mobile.Services;

public interface IRealtimeService
{
    // Raised on the UI thread when the server says that the leaderboard has changed.
    event Action? LeaderboardChanged;

    void Start();
    void Stop();
}