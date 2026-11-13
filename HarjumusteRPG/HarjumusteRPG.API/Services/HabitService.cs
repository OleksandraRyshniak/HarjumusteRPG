using HarjumusteRPG.API.Dtos;
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

    public async Task<PagedResult<HabitDto>> GetPagedAsync(int userId, HabitQueryParams q)
    {
        var criteria = new HabitSearchCriteria(q.Search, q.IsActive, q.IsDaily, q.SortBy, q.Descending, q.Page, q.PageSize);
        var (items, total) = await _habits.SearchAsync(userId, criteria);

        return new PagedResult<HabitDto>
        {
            Items = items.Select(ToDto).ToList(),
            Page = q.Page,
            PageSize = q.PageSize,
            TotalCount = total
        };
    }

    public async Task<HabitDto?> GetAsync(int id, int userId)
    {
        var habit = await _habits.GetByIdForUserAsync(id, userId);
        return habit is null ? null : ToDto(habit);
    }

    public async Task<ServiceResult<HabitDto>> CreateAsync(int userId, CreateHabitDto dto)
    {
        var name = dto.Name.Trim();
        if (await _habits.NameExistsAsync(userId, name))
            return new ServiceResult<HabitDto>(ServiceStatus.Conflict);

        var habit = new Habit
        {
            UserId = userId,
            Name = name,
            IsDaily = dto.IsDaily,
            DaysOfWeek = FormatDays(dto),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _habits.AddAsync(habit);
        await _habits.SaveChangesAsync();

        return new ServiceResult<HabitDto>(ServiceStatus.Ok, ToDto(habit));
    }

    public async Task<ServiceResult<HabitDto>> UpdateAsync(int id, int userId, UpdateHabitDto dto)
    {
        var habit = await _habits.GetByIdForUserAsync(id, userId);
        if (habit is null)
            return new ServiceResult<HabitDto>(ServiceStatus.NotFound);

        var name = dto.Name.Trim();
        if (await _habits.NameExistsAsync(userId, name, id))
            return new ServiceResult<HabitDto>(ServiceStatus.Conflict);

        habit.Name = name;
        habit.IsDaily = dto.IsDaily;
        habit.DaysOfWeek = FormatDays(dto);
        habit.IsActive = dto.IsActive;

        _habits.Update(habit);
        await _habits.SaveChangesAsync();

        return new ServiceResult<HabitDto>(ServiceStatus.Ok, ToDto(habit));
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var habit = await _habits.GetByIdForUserAsync(id, userId);
        if (habit is null) return false;

        _habits.Remove(habit);
        await _habits.SaveChangesAsync();
        return true;
    }

    private static string? FormatDays(CreateHabitDto dto) =>
        dto.IsDaily ? null : string.Join(",", dto.DaysOfWeek.Distinct().OrderBy(d => d));

    private static HabitDto ToDto(Habit h) => new()
    {
        Id = h.Id,
        Name = h.Name,
        IsDaily = h.IsDaily,
        DaysOfWeek = string.IsNullOrEmpty(h.DaysOfWeek)
            ? new List<int>()
            : h.DaysOfWeek.Split(',').Select(int.Parse).ToList(),
        IsActive = h.IsActive,
        CreatedAt = h.CreatedAt
    };
}