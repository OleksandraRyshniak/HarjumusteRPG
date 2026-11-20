namespace HarjumusteRPG.Mobile.Services;

public record AuthResult(bool Success, string? Error = null);

public interface IAuthService
{
    bool IsLoggedIn { get; }
    string? Username { get; }

    Task<bool> RestoreSessionAsync();
    Task<AuthResult> LoginAsync(string email, string password);
    Task<AuthResult> RegisterAsync(string username, string email, string password);
    void Logout();
}