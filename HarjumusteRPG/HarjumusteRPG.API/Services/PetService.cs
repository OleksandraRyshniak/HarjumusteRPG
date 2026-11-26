using HarjumusteRPG.API.Dtos;
using HarjumusteRPG.API.Models;
using HarjumusteRPG.API.Repositories;

namespace HarjumusteRPG.API.Services;

public class PetService : IPetService
{
    private readonly IUserRepository _users;
    private readonly IRepository<UserPet> _pets;
    private readonly ILogger<PetService> _logger;

    public PetService(IUserRepository users, IRepository<UserPet> pets, ILogger<PetService> logger)
    {
        _users = users;
        _pets = pets;
        _logger = logger;
    }

    // All shop pets with price and whether the user already has them.
    public async Task<ServiceResult<List<ShopPetDto>>> GetShopAsync(int userId)
    {
        var user = await _users.GetByIdAsync(userId);
        if (user is null)
            return new ServiceResult<List<ShopPetDto>>(ServiceStatus.NotFound);

        var owned = (await _pets.FindAsync(p => p.UserId == userId))
            .Select(p => p.PetKey)
            .ToHashSet();

        var list = GameRules.PetPrices
            .Select(p => new ShopPetDto
            {
                Key = p.Key,
                Price = p.Value,
                Owned = p.Value == 0 || owned.Contains(p.Key)
            })
            .ToList();

        return new ServiceResult<List<ShopPetDto>>(ServiceStatus.Ok, list);
    }

    // Buys a pet: checks the price and coins, saves the pet and takes the coins.
    public async Task<ServiceResult<BuyPetDto>> BuyAsync(int userId, string key)
    {
        if (!GameRules.PetPrices.TryGetValue(key, out var price))
            return new ServiceResult<BuyPetDto>(ServiceStatus.NotFound, null, "Unknown pet.");

        var user = await _users.GetByIdAsync(userId);
        if (user is null)
            return new ServiceResult<BuyPetDto>(ServiceStatus.NotFound);

        var alreadyOwned = price == 0 || (await _pets.FindAsync(p => p.UserId == userId && p.PetKey == key)).Count > 0;
        if (alreadyOwned)
            return new ServiceResult<BuyPetDto>(ServiceStatus.Conflict, null, "The user already has this pet.");

        if (user.Coins < price)
            return new ServiceResult<BuyPetDto>(ServiceStatus.Invalid, null, "Not enough coins.");

        user.Coins -= price;
        await _pets.AddAsync(new UserPet { UserId = userId, PetKey = key });
        await _pets.SaveChangesAsync();

        _logger.LogInformation("User {UserId} bought pet {PetKey} for {Price} coins", userId, key, price);

        return new ServiceResult<BuyPetDto>(ServiceStatus.Ok, new BuyPetDto
        {
            Key = key,
            Price = price,
            Coins = user.Coins
        });
    }
    // Feeding the pet costs coins.
    public async Task<ServiceResult<FeedDto>> FeedAsync(int userId)
    {
        var user = await _users.GetByIdAsync(userId);
        if (user is null)
            return new ServiceResult<FeedDto>(ServiceStatus.NotFound);

        if (user.Coins < GameRules.FeedCost)
            return new ServiceResult<FeedDto>(ServiceStatus.Invalid, null, "Not enough coins.");

        user.Coins -= GameRules.FeedCost;
        await _users.SaveChangesAsync();

        _logger.LogInformation("User {UserId} fed the pet, coins now {Coins}", userId, user.Coins);

        return new ServiceResult<FeedDto>(ServiceStatus.Ok, new FeedDto
        {
            CoinsSpent = GameRules.FeedCost,
            Coins = user.Coins
        });
    }

}