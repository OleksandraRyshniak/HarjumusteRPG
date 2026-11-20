using HarjumusteRPG.API.Dtos;
using HarjumusteRPG.API.Models;

namespace HarjumusteRPG.API.Repositories;

public record HabitSearchCriteria(
    string? Search,
    bool? IsActive,
    bool? IsDaily,
    HabitSortBy SortBy,
    bool Descending,
    int Page,
    int PageSize);

public interface IHabitRepository : IRepository<Habit>
{
    Task<List<Habit>> GetByUserAsync(int userId);
    Task<Habit?> GetByIdForUserAsync(int id, int userId);
    Task<(List<Habit> Items, int TotalCount)> SearchAsync(int userId, HabitSearchCriteria criteria);
    Task<bool> NameExistsAsync(int userId, string name, int? excludeId = null);
}