using HarjumusteRPG.API.Models;

namespace HarjumusteRPG.API.Repositories;

public interface IFriendshipRepository : IRepository<Friendship>
{
    Task<Friendship?> GetBetweenAsync(int userA, int userB);
    Task<Friendship?> GetWithUsersAsync(int id);
    Task<List<Friendship>> GetAcceptedForUserAsync(int userId);
    Task<List<Friendship>> GetIncomingAsync(int userId);
}