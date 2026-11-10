using HarjumusteRPG.API.Models;
using HarjumusteRPG.API.Repositories;

namespace HarjumusteRPG.API.Services;
public class HabitService : IHabitService
{
    private readonly IHabitRepository _habits;

    public HabitService(IHabitRepository habits)
    {
        _habits = habits;
    }

    public Task<List<Habit>> GetForUserAsync(int userId) => _habits.GetByUserAsync(userId);

    public Task<Habit?> GetAsync(int id, int userId) => _habits.GetByIdForUserAsync(id, userId);

    public async Task<Habit> CreateAsync(Habit habit)
    {
        await _habits.AddAsync(habit);
        await _habits.SaveChangesAsync();
        return habit;
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var habit = await _habits.GetByIdForUserAsync(id, userId);
        if (habit is null) return false;

        _habits.Remove(habit);
        await _habits.SaveChangesAsync();
        return true;
    }
}