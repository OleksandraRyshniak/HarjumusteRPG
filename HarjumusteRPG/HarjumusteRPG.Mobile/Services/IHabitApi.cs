using HarjumusteRPG.Mobile.Models;

namespace HarjumusteRPG.Mobile.Services;

public interface IHabitApi
{
    Task<ApiResult<PagedHabits>> GetPageAsync(string? search, bool? isActive, bool byName, bool descending, int page, int pageSize);
    Task<ApiResult<HabitInfo>> GetAsync(int id);
    Task<ApiResult<HabitInfo>> CreateAsync(HabitRequest request);
    Task<ApiResult<HabitInfo>> UpdateAsync(int id, HabitRequest request);
    Task<ApiResult<bool>> DeleteAsync(int id);
}