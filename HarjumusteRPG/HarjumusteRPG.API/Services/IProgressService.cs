using HarjumusteRPG.API.Dtos;

namespace HarjumusteRPG.API.Services
{
    public interface IProgressService
    {
        Task<ProgressDto?> GetAsync(int userId);
    }
}
