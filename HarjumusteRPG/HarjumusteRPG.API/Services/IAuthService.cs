using HarjumusteRPG.API.Dtos;

namespace HarjumusteRPG.API.Services;

public interface IAuthService
{
    /// <returns>null, если email или имя пользователя уже заняты.</returns>
    Task<UserDto?> RegisterAsync(RegisterDto dto);

    /// <returns>null, если email или пароль неверны.</returns>
    Task<UserDto?> LoginAsync(LoginDto dto);
}