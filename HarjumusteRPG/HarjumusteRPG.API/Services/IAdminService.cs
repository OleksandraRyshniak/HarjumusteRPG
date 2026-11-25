using HarjumusteRPG.API.Dtos;

namespace HarjumusteRPG.API.Services;

public interface IAdminService
{
    Task<PagedResult<AdminUserDto>> GetUsersAsync(AdminUserQueryParams query);
    Task<ServiceResult<bool>> DeleteUserAsync(int id, int currentAdminId);
    Task<List<AdminAchievementDto>> GetAchievementsAsync();
    Task<ServiceResult<AdminAchievementDto>> CreateAchievementAsync(AchievementEditDto dto);
    Task<ServiceResult<AdminAchievementDto>> UpdateAchievementAsync(int id, AchievementEditDto dto);
    Task<ServiceResult<bool>> DeleteAchievementAsync(int id);
}