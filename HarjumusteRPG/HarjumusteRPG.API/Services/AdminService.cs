using HarjumusteRPG.API.Dtos;
using HarjumusteRPG.API.Models;
using HarjumusteRPG.API.Repositories;

namespace HarjumusteRPG.API.Services;

public class AdminService : IAdminService
{
    private readonly IUserRepository _users;
    private readonly IFriendshipRepository _friendships;
    private readonly IRepository<Achievement> _achievements;
    private readonly ILeaderboardNotifier _notifier;
    private readonly ILogger<AdminService> _logger;

    public AdminService(
        IUserRepository users,
        IFriendshipRepository friendships,
        IRepository<Achievement> achievements,
        ILeaderboardNotifier notifier,
        ILogger<AdminService> logger)
    {
        _users = users;
        _friendships = friendships;
        _achievements = achievements;
        _notifier = notifier;
        _logger = logger;
    }

    public async Task<PagedResult<AdminUserDto>> GetUsersAsync(AdminUserQueryParams query)
    {
        var (items, total) = await _users.SearchAsync(query.Search, query.Role, query.Page, query.PageSize);

        return new PagedResult<AdminUserDto>
        {
            Items = items.Select(ToDto).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<ServiceResult<bool>> DeleteUserAsync(int id, int currentAdminId)
    {
        var user = await _users.GetByIdAsync(id);
        if (user is null)
            return new ServiceResult<bool>(ServiceStatus.NotFound);

        if (user.Id == currentAdminId)
            return new ServiceResult<bool>(ServiceStatus.Invalid, Error: "You cannot delete your own account.");

        if (user.Role != "User")
            return new ServiceResult<bool>(ServiceStatus.Invalid, Error: "Only accounts with the User role can be deleted.");

        // Friendships use Restrict, so they must be removed before the user
        var friendships = await _friendships.FindAsync(f => f.UserId == id || f.FriendId == id);
        var friendIds = friendships
            .Where(f => f.Status == FriendshipStatus.Accepted)
            .Select(f => f.UserId == id ? f.FriendId : f.UserId)
            .ToArray();

        foreach (var friendship in friendships)
            _friendships.Remove(friendship);

        // habits, completions and unlocked achievements are removed by cascade
        _users.Remove(user);
        await _users.SaveChangesAsync();

        _logger.LogInformation("Admin {AdminId} deleted user {UserId}", currentAdminId, id);

        // the leaderboards of the former friends must update
        await _notifier.NotifyAsync(friendIds);

        return new ServiceResult<bool>(ServiceStatus.Ok, true);
    }

    public async Task<List<AdminAchievementDto>> GetAchievementsAsync() =>
        (await _achievements.GetAllAsync()).OrderBy(a => a.Id).Select(ToDto).ToList();

    public async Task<ServiceResult<AdminAchievementDto>> CreateAchievementAsync(AchievementEditDto dto)
    {
        var name = dto.Name.Trim();
        if ((await _achievements.FindAsync(a => a.Name == name)).Count > 0)
            return new ServiceResult<AdminAchievementDto>(ServiceStatus.Conflict);

        var achievement = new Achievement
        {
            Name = name,
            Description = dto.Description.Trim(),
            Type = dto.Type,
            RequiredValue = dto.RequiredValue
        };

        await _achievements.AddAsync(achievement);
        await _achievements.SaveChangesAsync();

        return new ServiceResult<AdminAchievementDto>(ServiceStatus.Ok, ToDto(achievement));
    }

    public async Task<ServiceResult<AdminAchievementDto>> UpdateAchievementAsync(int id, AchievementEditDto dto)
    {
        var achievement = await _achievements.GetByIdAsync(id);
        if (achievement is null)
            return new ServiceResult<AdminAchievementDto>(ServiceStatus.NotFound);

        var name = dto.Name.Trim();
        if ((await _achievements.FindAsync(a => a.Name == name && a.Id != id)).Count > 0)
            return new ServiceResult<AdminAchievementDto>(ServiceStatus.Conflict);

        achievement.Name = name;
        achievement.Description = dto.Description.Trim();
        achievement.Type = dto.Type;
        achievement.RequiredValue = dto.RequiredValue;

        await _achievements.SaveChangesAsync();

        return new ServiceResult<AdminAchievementDto>(ServiceStatus.Ok, ToDto(achievement));
    }

    public async Task<ServiceResult<bool>> DeleteAchievementAsync(int id)
    {
        var achievement = await _achievements.GetByIdAsync(id);
        if (achievement is null)
            return new ServiceResult<bool>(ServiceStatus.NotFound);

        _achievements.Remove(achievement);
        await _achievements.SaveChangesAsync();

        return new ServiceResult<bool>(ServiceStatus.Ok, true);
    }

    private static AdminUserDto ToDto(User u) => new()
    {
        Id = u.Id,
        Username = u.Username,
        Email = u.Email,
        Role = u.Role,
        Level = u.Level,
        Xp = u.Xp,
        CurrentStreak = u.CurrentStreak,
        LongestStreak = u.LongestStreak,
        CreatedAt = u.CreatedAt
    };

    private static AdminAchievementDto ToDto(Achievement a) => new()
    {
        Id = a.Id,
        Name = a.Name,
        Description = a.Description,
        Type = a.Type.ToString(),
        RequiredValue = a.RequiredValue
    };
}