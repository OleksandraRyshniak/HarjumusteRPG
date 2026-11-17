using HarjumusteRPG.API.Dtos;

namespace HarjumusteRPG.API.Services
{
    public interface IStreakService
    {
        Task<List<StreakResetDto>> CheckAsync();
    }
}
