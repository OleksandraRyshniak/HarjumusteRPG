using HarjumusteRPG.API.Dtos;
using HarjumusteRPG.API.Models;
using HarjumusteRPG.API.Repositories;

namespace HarjumusteRPG.API.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;

    public AuthService(IUserRepository users)
    {
        _users = users;
    }

    public async Task<UserDto?> RegisterAsync(RegisterDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var username = dto.Username.Trim();

        if (await _users.GetByEmailAsync(email) is not null ||
            await _users.GetByUsernameAsync(username) is not null)
        {
            return null;
        }

        var user = new User
        {
            Username = username,
            Email = email,
            Role = "User", // Admin и Service создаются только через seed
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, 11)
        };

        await _users.AddAsync(user);
        await _users.SaveChangesAsync();

        return ToDto(user);
    }

    public async Task<UserDto?> LoginAsync(LoginDto dto)
    {
        var user = await _users.GetByEmailAsync(dto.Email.Trim().ToLowerInvariant());
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            return null;
        }

        return ToDto(user);
    }

    private static UserDto ToDto(User u) => new()
    {
        Id = u.Id,
        Username = u.Username,
        Email = u.Email,
        Role = u.Role,
        Xp = u.Xp,
        Level = u.Level
    };
}