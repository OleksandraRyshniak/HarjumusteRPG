using HarjumusteRPG.API.Data;
using HarjumusteRPG.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HarjumusteRPG.API.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email) =>
        await _set.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<User?> GetByUsernameAsync(string username) =>
        await _set.FirstOrDefaultAsync(u => u.Username == username);
}