using HarjumusteRPG.API.Data;
using HarjumusteRPG.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HarjumusteRPG.API.Repositories;

public class FriendshipRepository : Repository<Friendship>, IFriendshipRepository
{
    public FriendshipRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Friendship?> GetBetweenAsync(int userA, int userB) =>
        await _set.FirstOrDefaultAsync(f =>
            (f.UserId == userA && f.FriendId == userB) ||
            (f.UserId == userB && f.FriendId == userA));

    public async Task<Friendship?> GetWithUsersAsync(int id) =>
        await _set.Include(f => f.User).Include(f => f.Friend)
            .FirstOrDefaultAsync(f => f.Id == id);

    public async Task<List<Friendship>> GetAcceptedForUserAsync(int userId) =>
        await _set.Include(f => f.User).Include(f => f.Friend)
            .Where(f => f.Status == FriendshipStatus.Accepted &&
                        (f.UserId == userId || f.FriendId == userId))
            .ToListAsync();

    public async Task<List<Friendship>> GetIncomingAsync(int userId) =>
        await _set.Include(f => f.User)
            .Where(f => f.Status == FriendshipStatus.Pending && f.FriendId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
}