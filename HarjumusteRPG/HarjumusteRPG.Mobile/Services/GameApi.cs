using System.Net;
using System.Net.Http.Json;
using HarjumusteRPG.Mobile.Models;

namespace HarjumusteRPG.Mobile.Services;

public class GameApi : IGameApi
{
    private const string ConnectionError = "Serveriga ei saa ühendust. Kontrolli, et API töötab.";
    private const string UnknownError = "Midagi läks valesti. Proovi uuesti.";

    private readonly HttpClient _http;

    public GameApi(HttpClient http) => _http = http;

    public async Task<ApiResult<ProgressInfo>> GetProgressAsync()
    {
        try
        {
            var response = await _http.GetAsync("api/me/progress");
            if (!response.IsSuccessStatusCode)
                return new ApiResult<ProgressInfo>(default, ErrorFor(response.StatusCode));

            return new ApiResult<ProgressInfo>(await response.Content.ReadFromJsonAsync<ProgressInfo>());
        }
        catch (Exception ex) when (IsNetworkOrJson(ex))
        {
            return new ApiResult<ProgressInfo>(default, ErrorFor(ex));
        }
    }

    public async Task<ApiResult<List<TodayHabit>>> GetTodayAsync()
    {
        try
        {
            var response = await _http.GetAsync("api/habits?isActive=true&sortBy=name&page=1&pageSize=50");
            if (!response.IsSuccessStatusCode)
                return new ApiResult<List<TodayHabit>>(default, ErrorFor(response.StatusCode));

            var page = await response.Content.ReadFromJsonAsync<PagedHabits>();
            var weekday = (int)DateTime.Today.DayOfWeek; // 0 = Sunday, same as in the API
            var today = DateOnly.FromDateTime(DateTime.Today);

            var scheduled = (page?.Items ?? new())
                .Where(h => h.IsDaily || h.DaysOfWeek.Contains(weekday))
                .ToList();

            // One history request per habit: is there a completion with today's date?
            var done = await Task.WhenAll(scheduled.Select(h => IsDoneTodayAsync(h.Id, today)));

            var result = scheduled
                .Select((h, i) => new TodayHabit(h.Id, h.Name, done[i]))
                .ToList();

            return new ApiResult<List<TodayHabit>>(result);
        }
        catch (Exception ex) when (IsNetworkOrJson(ex))
        {
            return new ApiResult<List<TodayHabit>>(default, ErrorFor(ex));
        }
    }

    public async Task<ApiResult<CompleteResult>> CompleteAsync(int habitId)
    {
        try
        {
            var response = await _http.PostAsync($"api/habits/{habitId}/completions", null);
            if (!response.IsSuccessStatusCode)
                return new ApiResult<CompleteResult>(default, ErrorFor(response.StatusCode));

            return new ApiResult<CompleteResult>(await response.Content.ReadFromJsonAsync<CompleteResult>());
        }
        catch (Exception ex) when (IsNetworkOrJson(ex))
        {
            return new ApiResult<CompleteResult>(default, ErrorFor(ex));
        }
    }

    public async Task<ApiResult<List<CompletionInfo>>> GetHistoryAsync(int habitId, int days)
    {
        try
        {
            var response = await _http.GetAsync($"api/habits/{habitId}/completions?days={days}");
            if (!response.IsSuccessStatusCode)
                return new ApiResult<List<CompletionInfo>>(default, ErrorFor(response.StatusCode));

            var items = await response.Content.ReadFromJsonAsync<List<CompletionInfo>>() ?? new();
            return new ApiResult<List<CompletionInfo>>(items);
        }
        catch (Exception ex) when (IsNetworkOrJson(ex))
        {
            return new ApiResult<List<CompletionInfo>>(default, ErrorFor(ex));
        }
    }
    private async Task<bool> IsDoneTodayAsync(int habitId, DateOnly today)
    {
        var response = await _http.GetAsync($"api/habits/{habitId}/completions?days=1");
        if (!response.IsSuccessStatusCode)
            return false;

        var items = await response.Content.ReadFromJsonAsync<List<CompletionInfo>>();
        return items?.Any(c => c.Date == today) ?? false;
    }

    private static bool IsNetworkOrJson(Exception ex) =>
        ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException;

    private static string ErrorFor(Exception ex) =>
        ex is System.Text.Json.JsonException ? UnknownError : ConnectionError;

    private static string ErrorFor(HttpStatusCode status) => status switch
    {
        HttpStatusCode.Unauthorized => "Sessioon aegus. Logi uuesti sisse.",
        HttpStatusCode.Conflict => "See harjumus on täna juba tehtud.",
        HttpStatusCode.NotFound => "Harjumust ei leitud.",
        HttpStatusCode.BadRequest => "Seda harjumust ei saa täna märkida.",
        _ => UnknownError
    };
}