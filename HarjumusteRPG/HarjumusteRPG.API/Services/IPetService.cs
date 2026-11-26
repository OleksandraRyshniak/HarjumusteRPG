using HarjumusteRPG.API.Dtos;

namespace HarjumusteRPG.API.Services;

public interface IPetService
{
    Task<ServiceResult<List<ShopPetDto>>> GetShopAsync(int userId);
    Task<ServiceResult<FeedDto>> FeedAsync(int userId);
    Task<ServiceResult<BuyPetDto>> BuyAsync(int userId, string key);
}