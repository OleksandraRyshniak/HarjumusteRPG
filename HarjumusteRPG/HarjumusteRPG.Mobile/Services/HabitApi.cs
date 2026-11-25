using System.Net;
using System.Net.Http.Json;
using HarjumusteRPG.Mobile.Models;

namespace HarjumusteRPG.Mobile.Services;

public class HabitApi : IHabitApi
{
    private static string ConnectionError => AppResources.ErrConnection;
    private static string UnknownError => AppResources.ErrUnknown;

    private readonly HttpClient _http;

    public HabitApi(HttpClient http) => _http = http;

    public Task<ApiResult<PagedHabits>> GetPageAsync(string? search, bool? isActive, bool byName, bool descending, int page, int pageSize)
    {
        var url = $"api/habits?sortBy={(byName ? "name" : "createdAt")}&descending={descending.ToString().ToLowerInvariant()}&page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
            url += $"&search={Uri.EscapeDataString(search)}";
        if (isActive.HasValue)
            url += $"&isActive={isActive.Value.ToString().ToLowerInvariant()}";

        return RunAsync<PagedHabits>(() => _http.GetAsync(url));
    }

    public Task<ApiResult<HabitInfo>> GetAsync(int id) =>
        RunAsync<HabitInfo>(() => _http.GetAsync($"api/habits/{id}"));

    public Task<ApiResult<HabitInfo>> CreateAsync(HabitRequest request) =>
        RunAsync<HabitInfo>(() => _http.PostAsJsonAsync("api/habits", request));

    public Task<ApiResult<HabitInfo>> UpdateAsync(int id, HabitRequest request) =>
        RunAsync<HabitInfo>(() => _http.PutAsJsonAsync($"api/habits/{id}", request));

    public async Task<ApiResult<bool>> DeleteAsync(int id)
    {
        try
        {
            var response = await _http.DeleteAsync($"api/habits/{id}");
            return response.IsSuccessStatusCode
                ? new ApiResult<bool>(true)
                : new ApiResult<bool>(false, ErrorFor(response.StatusCode));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return new ApiResult<bool>(false, ConnectionError);
        }
    }

    // Sends a request and reads the JSON answer.
    private static async Task<ApiResult<T>> RunAsync<T>(Func<Task<HttpResponseMessage>> send)
    {
        try
        {
            var response = await send();
            if (!response.IsSuccessStatusCode)
                return new ApiResult<T>(default, ErrorFor(response.StatusCode));

            return new ApiResult<T>(await response.Content.ReadFromJsonAsync<T>());
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return new ApiResult<T>(default, ConnectionError);
        }
        catch (System.Text.Json.JsonException)
        {
            return new ApiResult<T>(default, UnknownError);
        }
    }

    private static string ErrorFor(HttpStatusCode status) => status switch
    {
        HttpStatusCode.Unauthorized => AppResources.ErrSessionExpired,
        HttpStatusCode.Conflict => AppResources.ErrHabitExists,
        HttpStatusCode.NotFound => AppResources.ErrHabitNotFound,
        HttpStatusCode.BadRequest => AppResources.ErrCheckData,
        _ => UnknownError
    };
}