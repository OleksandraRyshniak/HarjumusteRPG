using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace HarjumusteRPG.Mobile.Services;

// Keeps a WebSocket connection to /ws/leaderboard and reconnects if it breaks.
public class RealtimeService : IRealtimeService
{
    private readonly IAuthService _auth;
    private readonly HttpClient _http;
    private CancellationTokenSource? _cts;

    public RealtimeService(IAuthService auth, HttpClient http)
    {
        _auth = auth;
        _http = http;
    }

    public event Action? LeaderboardChanged;

    public void Start()
    {
        if (_cts is not null || !_auth.IsLoggedIn)
            return;

        _cts = new CancellationTokenSource();
        _ = RunAsync(_cts.Token);
    }

    public void Stop()
    {
        _cts?.Cancel();
        _cts = null;
    }

    private async Task RunAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var socket = new ClientWebSocket();
                await socket.ConnectAsync(BuildUri(), ct);
                await ReceiveAsync(socket, ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception)
            {
                // server is not reachable: try again after a pause
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task ReceiveAsync(ClientWebSocket socket, CancellationToken ct)
    {
        var buffer = new byte[4096];
        var text = new StringBuilder();

        while (socket.State == WebSocketState.Open && !ct.IsCancellationRequested)
        {
            var result = await socket.ReceiveAsync(buffer, ct);
            if (result.MessageType == WebSocketMessageType.Close)
                break;

            text.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
            if (!result.EndOfMessage)
                continue;

            var json = text.ToString();
            text.Clear();

            if (IsLeaderboardChanged(json))
                MainThread.BeginInvokeOnMainThread(() => LeaderboardChanged?.Invoke());
        }
    }

    private static bool IsLeaderboardChanged(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty("type", out var type)
                   && type.GetString() == "LeaderboardChanged";
        }
        catch (JsonException)
        {
            return false;
        }
    }

    // https://host:port -> wss://host:port/ws/leaderboard?access_token=...
    private Uri BuildUri()
    {
        var baseUri = _http.BaseAddress!;
        var scheme = baseUri.Scheme == Uri.UriSchemeHttps ? "wss" : "ws";
        var token = Uri.EscapeDataString(_auth.Token!);
        return new Uri($"{scheme}://{baseUri.Authority}/ws/leaderboard?access_token={token}");
    }
}