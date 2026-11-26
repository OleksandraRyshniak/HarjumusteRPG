using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using HarjumusteRPG.Mobile.Models;

namespace HarjumusteRPG.Mobile.Services;

public class AuthService : IAuthService
{
    private const string TokenKey = "auth_token";
    private const string UsernameKey = "auth_username";
   

    private static string ConnectionError => AppResources.ErrConnection;
    private static string UnknownError => AppResources.ErrUnknown;

    private readonly HttpClient _http;
    private string? _token;

    public AuthService(HttpClient http) => _http = http;

    public bool IsLoggedIn => !string.IsNullOrEmpty(_token);

    public string? Username { get; private set; }
    public string? Token => _token;


    public async Task<bool> RestoreSessionAsync()
    {
        if (IsLoggedIn)
            return true;

        var token = await ReadAsync(TokenKey);
        if (string.IsNullOrEmpty(token))
            return false;

        SetSession(token, await ReadAsync(UsernameKey));
        return true;
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/login", new LoginRequest(email, password));
            if (!response.IsSuccessStatusCode)
                return new AuthResult(false, ErrorFor(response.StatusCode));

            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var token = FindString(json.RootElement, "token");
            if (string.IsNullOrEmpty(token))
                return new AuthResult(false, UnknownError);

            var username = FindString(json.RootElement, "username") ?? email.Split('@')[0];

            SetSession(token, username);
            await WriteAsync(TokenKey, token);
            await WriteAsync(UsernameKey, username);

            return new AuthResult(true);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return new AuthResult(false, ConnectionError);
        }
        catch (JsonException)
        {
            return new AuthResult(false, UnknownError);
        }
    }

    public async Task<AuthResult> RegisterAsync(string username, string email, string password)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/register", new RegisterRequest(username, email, password));
            if (!response.IsSuccessStatusCode)
                return new AuthResult(false, ErrorFor(response.StatusCode));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return new AuthResult(false, ConnectionError);
        }

        // After a successful registration log in automatically.
        return await LoginAsync(email, password);
    }

    public void Logout()
    {
        _token = null;
        Username = null;
        _http.DefaultRequestHeaders.Authorization = null;
        Remove(TokenKey);
        Remove(UsernameKey);
    }

    private void SetSession(string token, string? username)
    {
        _token = token;
        Username = username;
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static string ErrorFor(HttpStatusCode status) => status switch
    {
        HttpStatusCode.Unauthorized => AppResources.ErrWrongCredentials,
        HttpStatusCode.Conflict => AppResources.ErrTaken,
        HttpStatusCode.BadRequest => AppResources.ErrCheckData,
        _ => UnknownError
    };

    // Finds a string property by name (case-insensitive), also inside nested objects.
    private static string? FindString(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return null;

        foreach (var property in element.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.String &&
                string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                return property.Value.GetString();
        }

        foreach (var property in element.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.Object)
            {
                var inner = FindString(property.Value, name);
                if (inner is not null)
                    return inner;
            }
        }

        return null;
    }

    // SecureStorage can fail on some Windows setups, so fall back to Preferences.
    private static async Task<string?> ReadAsync(string key)
    {
        try
        {
            return await SecureStorage.Default.GetAsync(key);
        }
        catch
        {
            var value = Preferences.Default.Get(key, string.Empty);
            return value.Length == 0 ? null : value;
        }
    }

    private static async Task WriteAsync(string key, string value)
    {
        try
        {
            await SecureStorage.Default.SetAsync(key, value);
        }
        catch
        {
            Preferences.Default.Set(key, value);
        }
    }

    private static void Remove(string key)
    {
        try { SecureStorage.Default.Remove(key); } catch { }
        Preferences.Default.Remove(key);
    }
}