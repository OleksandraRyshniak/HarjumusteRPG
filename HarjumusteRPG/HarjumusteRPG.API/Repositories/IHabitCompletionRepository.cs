using HarjumusteRPG.API.Models;

namespace HarjumusteRPG.API.Repositories;

public interface IHabitCompletionRepository : IRepository<HabitCompletion>
{
    Task<bool> ExistsAsync(int habitId, DateOnly date);
    Task<List<HabitCompletion>> GetByHabitAsync(int habitId, int days);
}