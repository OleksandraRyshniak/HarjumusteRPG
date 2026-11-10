using HarjumusteRPG.API.Data;
using HarjumusteRPG.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HarjumusteRPG.API.Repositories;

public class HabitCompletionRepository : Repository<HabitCompletion>, IHabitCompletionRepository
{
    public HabitCompletionRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<bool> ExistsAsync(int habitId, DateOnly date) =>
        await _set.AnyAsync(c => c.HabitId == habitId && c.Date == date);

    public async Task<List<HabitCompletion>> GetByHabitAsync(int habitId, int days)
    {
        var from = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-days);
        return await _set.Where(c => c.HabitId == habitId && c.Date >= from)
            .OrderByDescending(c => c.Date)
            .ToListAsync();
    }
}