using HarjumusteRPG.API.Dtos;
using HarjumusteRPG.API.Models;
using HarjumusteRPG.API.Repositories;

namespace HarjumusteRPG.API.Services;

public class AchievementService : IAchievementService
{
    private readonly IRepository<Achievement> _achievements;
    private readonly IRepository<UserAchievement> _unlocked;

    public AchievementService(IRepository<Achievement> achievements, IRepository<UserAchievement> unlocked)
    {
        _achievements = achievements;
        _unlocked = unlocked;
    }

    public async Task<List<AchievementDto>> GetForUserAsync(int userId)
    {
        var all = await _achievements.GetAllAsync();
        var unlocked = (await _unlocked.FindAsync(x => x.UserId == userId))
            .ToDictionary(x => x.AchievementId, x => x.UnlockedAt);

        return all.OrderBy(a => a.Id).Select(a => new AchievementDto
        {
            Id = a.Id,
            Name = a.Name,
            Description = a.Description,
            RequiredValue = a.RequiredValue,
            Unlocked = unlocked.ContainsKey(a.Id),
            UnlockedAt = unlocked.TryGetValue(a.Id, out var at) ? (DateTime?)at : null
        }).ToList();
    }

    public async Task<List<AchievementDto>> UnlockNewAsync(User user, int totalCompletions)
    {
        var all = await _achievements.GetAllAsync();
        var alreadyUnlocked = (await _unlocked.FindAsync(x => x.UserId == user.Id))
            .Select(x => x.AchievementId)
            .ToHashSet();

        var result = new List<AchievementDto>();

        foreach (var a in all.OrderBy(a => a.Id))
        {
            if (alreadyUnlocked.Contains(a.Id)) continue;
            if (!GameRules.IsEarned(a, user.Level, user.CurrentStreak, totalCompletions)) continue;

            var now = DateTime.UtcNow;
            await _unlocked.AddAsync(new UserAchievement { UserId = user.Id, AchievementId = a.Id, UnlockedAt = now });

            result.Add(new AchievementDto
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Description,
                RequiredValue = a.RequiredValue,
                Unlocked = true,
                UnlockedAt = now
            });
        }

        return result;
    }
}