using HarjumusteRPG.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HarjumusteRPG.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    public DbSet<User> Users => Set<User>();
    public DbSet<Habit> Habits => Set<Habit>();
    public DbSet<HabitCompletion> HabitCompletions => Set<HabitCompletion>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();
    public DbSet<Friendship> Friendships => Set<Friendship>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e =>
        {
            e.Property(x => x.Username).HasMaxLength(50);
            e.Property(x => x.Email).HasMaxLength(100);
            e.Property(x => x.Role).HasMaxLength(20);
            e.HasIndex(x => x.Email).IsUnique();
            e.HasIndex(x => x.Username).IsUnique();
        });

        b.Entity<Habit>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(100);
            e.HasOne(x => x.User).WithMany(u => u.Habits)
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<HabitCompletion>(e =>
        {
            e.HasIndex(x => new { x.HabitId, x.Date }).IsUnique();
            e.HasOne(x => x.Habit).WithMany(h => h.Completions)
                .HasForeignKey(x => x.HabitId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<UserAchievement>(e =>
        {
            e.HasIndex(x => new { x.UserId, x.AchievementId }).IsUnique();
            e.HasOne(x => x.User).WithMany(u => u.UserAchievements)
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Achievement).WithMany(a => a.UserAchievements)
                .HasForeignKey(x => x.AchievementId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Friendship>(e =>
        {
            e.HasIndex(x => new { x.UserId, x.FriendId }).IsUnique();
            e.HasOne(x => x.User).WithMany()
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Friend).WithMany()
                .HasForeignKey(x => x.FriendId).OnDelete(DeleteBehavior.Restrict);
        });

        var created = new DateTime(2026, 11, 9, 0, 0, 0, DateTimeKind.Utc);

        // Dev-only seed accounts: Admin123! and Service123!
        b.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Username = "admin",
                Email = "admin@sproutly.local",
                Role = "Admin",
                PasswordHash = "$2b$11$k8fDIZHKppRuZV75my8dc.r0ns4zCq7DXr8YU3z8ZyjDxaRf/8UgC",
                CreatedAt = created
            },
            new User
            {
                Id = 2,
                Username = "service",
                Email = "service@sproutly.local",
                Role = "Service",
                PasswordHash = "$2b$11$qkitA4RIMQ0kNp.vc3Z.Cu.MEtCoV8WlYb.27iPmhk48hvlh4AW5K",
                CreatedAt = created
            });

        b.Entity<Achievement>().HasData(
            new Achievement { Id = 1, Name = "Esimene samm", Description = "Märgi esimene harjumus tehtuks", RequiredValue = 1 },
            new Achievement { Id = 2, Name = "7 päeva järjest", Description = "Hoia seeriat 7 päeva", RequiredValue = 7 },
            new Achievement { Id = 3, Name = "30 päeva järjest", Description = "Hoia seeriat 30 päeva", RequiredValue = 30 },
            new Achievement { Id = 4, Name = "Tase 5", Description = "Jõua tasemeni 5", RequiredValue = 5 },
            new Achievement { Id = 5, Name = "100 täitmist", Description = "Märgi 100 harjumust tehtuks", RequiredValue = 100 });
    }
}