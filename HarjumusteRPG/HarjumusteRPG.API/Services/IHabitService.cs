using HarjumusteRPG.API.Dtos;

namespace HarjumusteRPG.API.Services;

public interface IHabitService
{
    Task<PagedResult<HabitDto>> GetPagedAsync(int userId, HabitQueryParams query);
    Task<HabitDto?> GetAsync(int id, int userId);
    Task<ServiceResult<HabitDto>> CreateAsync(int userId, CreateHabitDto dto);
    Task<ServiceResult<HabitDto>> UpdateAsync(int id, int userId, UpdateHabitDto dto);
    Task<bool> DeleteAsync(int id, int userId);
}