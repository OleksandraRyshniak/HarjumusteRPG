using System.Net;
using System.Net.Http.Json;
using HarjumusteRPG.Mobile.Models;

namespace HarjumusteRPG.Mobile.Services;

public class FriendApi : IFriendApi
{
    private static string ConnectionError => AppResources.ErrConnection;
    private static string UnknownError => AppResources.ErrUnknown;

    private readonly HttpClient _http;

    public FriendApi(HttpClient http) => _http = http;

    public Task<ApiResult<List<LeaderboardEntry>>> GetLeaderboardAsync() =>
        RunAsync<List<LeaderboardEntry>>(() => _http.GetAsync("api/friends/leaderboard"));

    public Task<ApiResult<List<FriendInfo>>> GetFriendsAsync() =>
        RunAsync<List<FriendInfo>>(() => _http.GetAsync("api/friends"));

    public Task<ApiResult<List<FriendRequestInfo>>> GetRequestsAsync() =>
        RunAsync<List<FriendRequestInfo>>(() => _http.GetAsync("api/friends/requests"));

    public Task<ApiResult<FriendRequestInfo>> SendRequestAsync(string username) =>
        RunAsync<FriendRequestInfo>(
            () => _http.PostAsJsonAsync("api/friends/requests", new { username }),
            status => status switch
            {
                HttpStatusCode.NotFound => AppResources.ErrUserNotFound,
                HttpStatusCode.Conflict => AppResources.ErrInviteExists,
                HttpStatusCode.BadRequest => AppResources.ErrFriendNameCheck,
                _ => DefaultError(status)
            });

    public Task<ApiResult<FriendInfo>> AcceptAsync(int requestId) =>
        RunAsync<FriendInfo>(
            () => _http.PostAsync($"api/friends/requests/{requestId}/accept", null),
            status => status == HttpStatusCode.NotFound ? AppResources.ErrInviteNotFound : DefaultError(status));

    public Task<ApiResult<bool>> DeclineAsync(int requestId) =>
        RunAsync<bool>(
            () => _http.DeleteAsync($"api/friends/requests/{requestId}"),
            status => status == HttpStatusCode.NotFound ? AppResources.ErrInviteNotFound : DefaultError(status));

    public Task<ApiResult<bool>> RemoveAsync(int friendId) =>
        RunAsync<bool>(
            () => _http.DeleteAsync($"api/friends/{friendId}"),
            status => status == HttpStatusCode.NotFound ? AppResources.ErrFriendNotFound : DefaultError(status));

    // Sends a request. Answers without a body (204) give true, other answers are read as JSON.
    private static async Task<ApiResult<T>> RunAsync<T>(
        Func<Task<HttpResponseMessage>> send, Func<HttpStatusCode, string>? errorFor = null)
    {
        try
        {
            using var response = await send();
            if (!response.IsSuccessStatusCode)
                return new ApiResult<T>(default, (errorFor ?? DefaultError)(response.StatusCode));

            if (typeof(T) == typeof(bool))
                return new ApiResult<T>((T)(object)true);

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

    private static string DefaultError(HttpStatusCode status) => status switch
    {
        HttpStatusCode.Unauthorized => AppResources.ErrSessionExpired,
        _ => UnknownError
    };
}