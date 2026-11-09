using Microsoft.EntityFrameworkCore;

namespace HarjumusteRPG.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
}