using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using HarjumusteRPG.Admin.Models;
using Microsoft.AspNetCore.Authentication;

namespace HarjumusteRPG.Admin.Services;

public record ApiResult<T>(bool Ok, T? Value = default, string? Error = null);

/// <summary>All calls to the Web API. The JWT is taken from the sign-in cookie.</summary>
public class AdminApiClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _accessor;

    public AdminApiClient(HttpClient http, IHttpContextAccessor accessor)
    {
        _http = http;
        _accessor = accessor;
    }

    public Task<ApiResult<AuthResponse>> LoginAsync(string email, string password) =>
        SendAsync<AuthResponse>(HttpMethod.Post, "api/auth/login", new { email, password }, authorize: false);

    public Task<ApiResult<PagedResult<AdminUser>>> GetUsersAsync(string? search, string? role, int page, int pageSize = 10)
    {
        var url = $"api/admin/users?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search)) url += "&search=" + Uri.EscapeDataString(search);
        if (!string.IsNullOrWhiteSpace(role)) url += "&role=" + Uri.EscapeDataString(role);
        return SendAsync<PagedResult<AdminUser>>(HttpMethod.Get, url);
    }

    public Task<ApiResult<bool>> DeleteUserAsync(int id) =>
        SendAsync<bool>(HttpMethod.Delete, $"api/admin/users/{id}");

    public Task<ApiResult<List<Achievement>>> GetAchievementsAsync() =>
        SendAsync<List<Achievement>>(HttpMethod.Get, "api/admin/achievements");

    public Task<ApiResult<Achievement>> CreateAchievementAsync(AchievementEdit model) =>
        SendAsync<Achievement>(HttpMethod.Post, "api/admin/achievements", ToBody(model));

    public Task<ApiResult<Achievement>> UpdateAchievementAsync(int id, AchievementEdit model) =>
        SendAsync<Achievement>(HttpMethod.Put, $"api/admin/achievements/{id}", ToBody(model));

    public Task<ApiResult<bool>> DeleteAchievementAsync(int id) =>
        SendAsync<bool>(HttpMethod.Delete, $"api/admin/achievements/{id}");

    private static object ToBody(AchievementEdit m) => new
    {
        name = m.Name,
        description = m.Description ?? string.Empty,
        type = m.Type,
        requiredValue = m.RequiredValue
    };

    private async Task<ApiResult<T>> SendAsync<T>(HttpMethod method, string url, object? body = null, bool authorize = true)
    {
        using var request = new HttpRequestMessage(method, url);
        if (body is not null)
            request.Content = JsonContent.Create(body);

        if (authorize)
        {
            var token = await _accessor.HttpContext!.GetTokenAsync("access_token");
            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        try
        {
            using var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return new ApiResult<T>(false, default, await ReadErrorAsync(response));

            if (response.StatusCode == HttpStatusCode.NoContent || response.Content.Headers.ContentLength == 0)
                return new ApiResult<T>(true);

            var value = await response.Content.ReadFromJsonAsync<T>(JsonSerializerOptions.Web);
            return new ApiResult<T>(true, value);
        }
        catch (HttpRequestException)
        {
            return new ApiResult<T>(false, default, "Cannot reach the API. Is it running?");
        }
    }

    // The API answers with ProblemDetails; show its "detail" or the validation messages
    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        try
        {
            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var root = doc.RootElement;

            if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
            {
                var messages = errors.EnumerateObject()
                    .SelectMany(p => p.Value.EnumerateArray().Select(e => e.GetString()));
                return string.Join(" ", messages);
            }

            if (root.TryGetProperty("detail", out var detail) && !string.IsNullOrWhiteSpace(detail.GetString()))
                return detail.GetString()!;
        }
        catch (Exception)
        {
            // not JSON, use the status code below
        }

        return response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Session expired. Log out and log in again.",
            HttpStatusCode.Forbidden => "You do not have permission.",
            HttpStatusCode.NotFound => "Not found.",
            _ => $"Request failed ({(int)response.StatusCode})."
        };
    }
}