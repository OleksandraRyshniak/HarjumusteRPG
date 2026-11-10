using HarjumusteRPG.API.Data;
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
}