using HarjumusteRPG.API.Dtos;

namespace HarjumusteRPG.API.Services;

public interface IDailyGiftService
{
    Task<ServiceResult<DailyGiftDto>> ClaimAsync(int userId);
}