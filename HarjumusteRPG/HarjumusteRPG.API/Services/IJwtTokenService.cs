using HarjumusteRPG.API.Models;

namespace HarjumusteRPG.API.Services;

public interface IJwtTokenService
{
    string CreateToken(User user);
}