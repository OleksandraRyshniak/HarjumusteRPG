using System.ComponentModel.DataAnnotations;

namespace HarjumusteRPG.Admin.Models;

public class UserInfo
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public UserInfo User { get; set; } = new();
}

public class AdminUser
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

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}

public class Achievement
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int RequiredValue { get; set; }
}

public class AchievementEdit
{
    [Display(Name = "Nimi")]
    [Required(ErrorMessage = "Sisesta nimi.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Nimi peab olema 2–100 tähemärki.")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Kirjeldus")]
    [StringLength(300, ErrorMessage = "Kirjeldus võib olla kuni 300 tähemärki.")]
    public string? Description { get; set; }

    [Display(Name = "Tüüp")]
    [Required(ErrorMessage = "Vali tüüp.")]
    public string Type { get; set; } = "TotalCompletions";

    [Display(Name = "Nõutud väärtus")]
    [Range(1, 100000, ErrorMessage = "Väärtus peab olema 1 kuni 100000.")]
    public int RequiredValue { get; set; } = 1;
}

public class LoginViewModel
{
    [Display(Name = "E-post")]
    [Required(ErrorMessage = "Sisesta e-post.")]
    [EmailAddress(ErrorMessage = "E-posti aadress ei ole korrektne.")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Parool")]
    [Required(ErrorMessage = "Sisesta parool.")]
    public string Password { get; set; } = string.Empty;
}