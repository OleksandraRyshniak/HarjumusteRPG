using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using HarjumusteRPG.API.Models;

namespace HarjumusteRPG.API.Dtos;

public class AdminUserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int Level { get; set; }
    public int Xp { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminUserQueryParams
{
    /// <summary>Part of the username or email.</summary>
    public string? Search { get; set; }

    /// <summary>User, Admin or Service.</summary>
    public string? Role { get; set; }

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 50)]
    public int PageSize { get; set; } = 10;
}

public class AchievementEditDto
{
    /// <example>Kuu järjest</example>
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    /// <example>Hoia seeriat 30 päeva</example>
    [StringLength(300)]
    public string Description { get; set; } = string.Empty;

    /// <summary>TotalCompletions, Streak or Level.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [EnumDataType(typeof(AchievementType))]
    public AchievementType Type { get; set; } = AchievementType.TotalCompletions;

    /// <example>30</example>
    [Range(1, 100000)]
    public int RequiredValue { get; set; }
}

public class AdminAchievementDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int RequiredValue { get; set; }
}