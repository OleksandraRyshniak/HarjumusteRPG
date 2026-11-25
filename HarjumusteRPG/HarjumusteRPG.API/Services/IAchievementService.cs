using HarjumusteRPG.API.Dtos;
using HarjumusteRPG.API.Models;

namespace HarjumusteRPG.API.Services;

public interface IAchievementService
{
    Task<List<AchievementDto>> GetForUserAsync(int userId);

    /// <summary>Adds newly earned achievements to the context (saved by the caller).</summary>
    Task<List<AchievementDto>> UnlockNewAsync(User user, int totalCompletions);
}