using HarjumusteRPG.API.Dtos;

namespace HarjumusteRPG.API.Services
{
    public interface ICompletionService
    {
        Task<ServiceResult<CompleteHabitResultDto>> CompleteAsync(int habitId, int userId);
        Task<List<CompletionDto>?> GetHistoryAsync(int habitId, int userId, int days);
    }
}
