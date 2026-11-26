
using CommunityToolkit.Mvvm.ComponentModel;
using HarjumusteRPG.Mobile.Models;
using System.Net;
using System.Net.Http.Json;

namespace HarjumusteRPG.Mobile.Services;

public class GameApi : IGameApi
{
    private static string ConnectionError => AppResources.ErrConnection;
    private static string UnknownError => AppResources.ErrUnknown;

    private readonly HttpClient _http;

    public GameApi(HttpClient http) => _http = http;

    public async Task<ApiResult<ProgressInfo>> GetProgressAsync()
    {
        try
        {
            var response = await _http.GetAsync("api/me/progress");
            if (!response.IsSuccessStatusCode)
                return new ApiResult<ProgressInfo>(default, ErrorFor(response.StatusCode));

            var progress = await response.Content.ReadFromJsonAsync<ProgressInfo>();
            if (progress is not null)
            {
                AppClock.SetServerToday(progress.Today);
                Player.Current.SetCoins(progress.Coins);
                Player.Current.SetLevel(progress.Level);
            }
            return new ApiResult<ProgressInfo>(progress);
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
            var weekday = (int)AppClock.Today.DayOfWeek; // 0 = Sunday, same as in the API
            var today = AppClock.Today;

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

    public async Task<ApiResult<DailyGiftResult>> ClaimDailyGiftAsync()
    {
        try
        {
            var response = await _http.PostAsync("api/me/daily-gift", null);
            if (response.StatusCode == HttpStatusCode.Conflict)
                return new ApiResult<DailyGiftResult>(default, AppResources.ErrGiftClaimed);
            if (!response.IsSuccessStatusCode)
                return new ApiResult<DailyGiftResult>(default, ErrorFor(response.StatusCode));

            return new ApiResult<DailyGiftResult>(await response.Content.ReadFromJsonAsync<DailyGiftResult>());
        }
        catch (Exception ex) when (IsNetworkOrJson(ex))
        {
            return new ApiResult<DailyGiftResult>(default, ErrorFor(ex));
        }
    }

    public async Task<ApiResult<List<ShopPet>>> GetShopPetsAsync()
    {
        try
        {
            var response = await _http.GetAsync("api/me/pets");
            if (!response.IsSuccessStatusCode)
                return new ApiResult<List<ShopPet>>(default, ErrorFor(response.StatusCode));

            var items = await response.Content.ReadFromJsonAsync<List<ShopPet>>() ?? new();
            return new ApiResult<List<ShopPet>>(items);
        }
        catch (Exception ex) when (IsNetworkOrJson(ex))
        {
            return new ApiResult<List<ShopPet>>(default, ErrorFor(ex));
        }
    }

    public async Task<ApiResult<BuyPetResult>> BuyPetAsync(string key)
    {
        try
        {
            var response = await _http.PostAsync($"api/me/pets/{key}/buy", null);
            if (response.StatusCode == HttpStatusCode.BadRequest)
                return new ApiResult<BuyPetResult>(default, AppResources.ErrNotEnoughCoins);
            if (response.StatusCode == HttpStatusCode.Conflict)
                return new ApiResult<BuyPetResult>(default, AppResources.ErrPetOwned);
            if (!response.IsSuccessStatusCode)
                return new ApiResult<BuyPetResult>(default, ErrorFor(response.StatusCode));

            return new ApiResult<BuyPetResult>(await response.Content.ReadFromJsonAsync<BuyPetResult>());
        }
        catch (Exception ex) when (IsNetworkOrJson(ex))
        {
            return new ApiResult<BuyPetResult>(default, ErrorFor(ex));
        }
    }

    public async Task<ApiResult<FeedResult>> FeedPetAsync()
    {
        try
        {
            var response = await _http.PostAsync("api/me/pet/feed", null);
            if (response.StatusCode == HttpStatusCode.BadRequest)
                return new ApiResult<FeedResult>(default, AppResources.ErrNotEnoughCoins);
            if (!response.IsSuccessStatusCode)
                return new ApiResult<FeedResult>(default, ErrorFor(response.StatusCode));

            return new ApiResult<FeedResult>(await response.Content.ReadFromJsonAsync<FeedResult>());
        }
        catch (Exception ex) when (IsNetworkOrJson(ex))
        {
            return new ApiResult<FeedResult>(default, ErrorFor(ex));
        }
    }

    public Task<ApiResult<DemoTimeResult>> DemoNextDayAsync() => DemoAsync("api/demo/next-day");

    public Task<ApiResult<DemoTimeResult>> DemoResetAsync() => DemoAsync("api/demo/reset");

    private async Task<ApiResult<DemoTimeResult>> DemoAsync(string url)
    {
        try
        {
            var response = await _http.PostAsync(url, null);
            if (!response.IsSuccessStatusCode)
                return new ApiResult<DemoTimeResult>(default, ErrorFor(response.StatusCode));

            var result = await response.Content.ReadFromJsonAsync<DemoTimeResult>();
            if (result is not null)
                AppClock.SetServerToday(result.Today);
            return new ApiResult<DemoTimeResult>(result);
        }
        catch (Exception ex) when (IsNetworkOrJson(ex))
        {
            return new ApiResult<DemoTimeResult>(default, ErrorFor(ex));
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
        HttpStatusCode.Unauthorized => AppResources.ErrSessionExpired,
        HttpStatusCode.Conflict => AppResources.ErrAlreadyDone,
        HttpStatusCode.NotFound => AppResources.ErrHabitNotFound,
        HttpStatusCode.BadRequest => AppResources.ErrCannotMarkToday,
        _ => UnknownError
    }; }

    // Coins and level of the logged-in user, shared by the headers of all main pages.
    // GameApi updates them after every answer that contains coins or level.
    public class Player : ObservableObject
    {
        public static Player Current { get; } = new();

        private string _coinsText = "0";
        private string _levelText = "LV 1";

        public string CoinsText
        {
            get => _coinsText;
            set => SetProperty(ref _coinsText, value);
        }

        public string LevelText
        {
            get => _levelText;
            set => SetProperty(ref _levelText, value);
        }

        public void SetCoins(int coins) =>
            MainThread.BeginInvokeOnMainThread(() => CoinsText = coins.ToString());

        public void SetLevel(int level) =>
            MainThread.BeginInvokeOnMainThread(() => LevelText = $"LV {level}");
    }

