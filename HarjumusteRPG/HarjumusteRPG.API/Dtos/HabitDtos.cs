using System.ComponentModel.DataAnnotations;

namespace HarjumusteRPG.API.Dtos;

public enum HabitSortBy
{
    CreatedAt,
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
    /// <summary>Part of the habit name.</summary>
    public string? Search { get; set; }

    public bool? IsActive { get; set; }
    public bool? IsDaily { get; set; }
    public HabitSortBy SortBy { get; set; } = HabitSortBy.CreatedAt;
    public bool Descending { get; set; }

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

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