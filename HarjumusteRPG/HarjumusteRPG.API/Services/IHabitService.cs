using HarjumusteRPG.API.Models;

namespace HarjumusteRPG.API.Services;
public interface IHabitService
{
    Task<List<Habit>> GetForUserAsync(int userId);
    Task<Habit?> GetAsync(int id, int userId);
    Task<Habit> CreateAsync(Habit habit);
    Task<bool> DeleteAsync(int id, int userId);
}