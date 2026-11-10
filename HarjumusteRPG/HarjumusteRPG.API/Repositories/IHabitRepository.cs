using HarjumusteRPG.API.Models;

namespace HarjumusteRPG.API.Repositories;

public interface IHabitRepository : IRepository<Habit>
{
    Task<List<Habit>> GetByUserAsync(int userId);
    Task<Habit?> GetByIdForUserAsync(int id, int userId);
}