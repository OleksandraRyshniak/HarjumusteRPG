namespace HarjumusteRPG.Mobile.Models;

public record LoginRequest(string Email, string Password);

public record RegisterRequest(string Username, string Email, string Password);

public record UserInfo(int Id, string Username, string Email, string Role, int Xp, int Level);

public record AuthResponse(string Token, UserInfo User);