using HarjumusteRPG.API.Data;
using HarjumusteRPG.API.Dtos;
using HarjumusteRPG.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HarjumusteRPG.API.Repositories;

public class HabitRepository : Repository<Habit>, IHabitRepository
{
    public HabitRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<Habit>> GetByUserAsync(int userId) =>
        await _set.Where(h => h.UserId == userId).ToListAsync();

    public async Task<Habit?> GetByIdForUserAsync(int id, int userId) =>
        await _set.FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

    public async Task<(List<Habit> Items, int TotalCount)> SearchAsync(int userId, HabitSearchCriteria c)
    {
        var query = _set.AsNoTracking().Where(h => h.UserId == userId);

        if (!string.IsNullOrWhiteSpace(c.Search))
        {
            var term = c.Search.Trim();
            query = query.Where(h => h.Name.Contains(term));
        }

        if (c.IsActive.HasValue)
            query = query.Where(h => h.IsActive == c.IsActive.Value);

        if (c.IsDaily.HasValue)
            query = query.Where(h => h.IsDaily == c.IsDaily.Value);

        var total = await query.CountAsync();

        IOrderedQueryable<Habit> ordered = c.SortBy == HabitSortBy.Name
            ? (c.Descending ? query.OrderByDescending(h => h.Name) : query.OrderBy(h => h.Name))
            : (c.Descending ? query.OrderByDescending(h => h.CreatedAt) : query.OrderBy(h => h.CreatedAt));

        var items = await ordered
            .ThenBy(h => h.Id)
            .Skip((c.Page - 1) * c.PageSize)
            .Take(c.PageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<bool> NameExistsAsync(int userId, string name, int? excludeId = null)
    {
        var lower = name.ToLower();
        return await _set.AnyAsync(h =>
            h.UserId == userId &&
            h.Name.ToLower() == lower &&
            (excludeId == null || h.Id != excludeId));
    }
}