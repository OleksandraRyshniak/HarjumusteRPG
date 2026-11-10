using HarjumusteRPG.API.Models;

namespace HarjumusteRPG.API.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
}