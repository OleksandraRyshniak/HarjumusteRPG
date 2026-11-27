using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HarjumusteRPG.API.Dtos;

/// <summary>Field used to sort the habit list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HabitSortBy
{
    /// <summary>By creation time.</summary>
    CreatedAt,

    /// <summary>By habit name (A–Z).</summary>
    Name
}

public class HabitDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsDaily { get; set; }

    /// <summary>
    /// Weekdays: 0 = Sunday ... 6 = Saturday. 
    /// Empty for daily habits.
    /// </summary>
    public List<int> DaysOfWeek { get; set; } = new();

    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateHabitDto : IValidatableObject
{
    /// <example>Drink water </example>
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    /// <example>false</example>
    public bool IsDaily { get; set; } = true;

    /// <summary>
    /// Required when IsDaily is false.
    /// Values 0 (Sunday) to 6 (Saturday).
    /// </summary>
    /// <example>[1, 3, 5]</example>
    public List<int> DaysOfWeek { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var field = new[] { nameof(DaysOfWeek) };

        if (!IsDaily && DaysOfWeek.Count == 0)
            yield return new ValidationResult("Choose at least one weekday for a non-daily habit.", field);

        if (DaysOfWeek.Any(d => d < 0 || d > 6))
            yield return new ValidationResult("Weekdays must be between 0 (Sunday) and 6 (Saturday).", field);

        if (DaysOfWeek.Distinct().Count() != DaysOfWeek.Count)
            yield return new ValidationResult("Weekdays must not repeat.", field);
    }
}

public class UpdateHabitDto : CreateHabitDto
{
    /// <example>true</example>
    public bool IsActive { get; set; } = true;
}

public class HabitQueryParams
{
    /// <summary>Part of the habit name. Leave empty to show all habits.</summary>
    /// <example>water</example>
    public string? Search { get; set; }

    /// <summary>true = only active habits, false = only inactive. Empty = all.</summary>
    public bool? IsActive { get; set; }

    /// <summary>true = only daily habits, false = only weekday habits. Empty = all.</summary>
    public bool? IsDaily { get; set; }

    /// <summary>Sort field: CreatedAt (by creation time) or Name (A–Z). Default: CreatedAt.</summary>
    public HabitSortBy SortBy { get; set; } = HabitSortBy.CreatedAt;

    /// <summary>true = reverse order (newest first or Z–A). Default: false.</summary>
    public bool Descending { get; set; }

    /// <summary>Page number, starting from 1.</summary>
    /// <example>1</example>
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    /// <summary>Habits per page (1–50). Default: 10.</summary>
    /// <example>10</example>
    [Range(1, 50)]
    public int PageSize { get; set; } = 10;
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}