using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace HarjumusteRPG.Worker;

public record StreakReset(int UserId, string Username, int PreviousStreak);

internal record LoginRequest(string Email, string Password);
internal record LoginResponse(string Token);

/// <summary>Suhtleb ainult API-ga HTTP kaudu (Worker ei puutu andmebaasi).</summary>
public class StreakApiClient
{
    private readonly HttpClient _http;
    private readonly WorkerOptions _options;
    private string? _token;

    public StreakApiClient(HttpClient http, IOptions<WorkerOptions> options)
    {
        _http = http;
        _options = options.Value;
    }

    private async Task LoginAsync(CancellationToken ct)
    {
        var response = await _http.PostAsJsonAsync(
            "api/auth/login", new LoginRequest(_options.Email, _options.Password), ct);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>(ct);
        _token = body?.Token ?? throw new InvalidOperationException("API ei tagastanud tokenit.");
    }

    private async Task<HttpResponseMessage> SendCheckAsync(CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/streaks/check");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        return await _http.SendAsync(request, ct);
    }

    public async Task<List<StreakReset>> CheckStreaksAsync(CancellationToken ct)
    {
        if (_token is null)
            await LoginAsync(ct);

        var response = await SendCheckAsync(ct);

        // Token aegus: logi uuesti sisse ja proovi veel kord
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            response.Dispose();
            await LoginAsync(ct);
            response = await SendCheckAsync(ct);
        }

        using (response)
        {
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<StreakReset>>(ct) ?? [];
        }
    }
}