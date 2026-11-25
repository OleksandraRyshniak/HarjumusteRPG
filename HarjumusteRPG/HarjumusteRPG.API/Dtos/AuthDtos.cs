using System.ComponentModel.DataAnnotations;

namespace HarjumusteRPG.API.Dtos;

public class RegisterDto
{
    /// <example>alexa</example>
    [Required, StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    /// <example>alexa@test.com</example>
    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = string.Empty;

    /// <example>Test123!</example>
    [Required]
    [StringLength(100, MinimumLength = 6)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d\s]).+$",
    ErrorMessage = "Parool peab sisaldama väiketähte, suurtähte, numbrit ja erimärki.")]
    public string Password { get; set; } = string.Empty;
}

public class LoginDto
{
    /// <example>alexa@test.com</example>
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <example>Test123!</example>
    [Required]
    public string Password { get; set; } = string.Empty;
}

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int Xp { get; set; }
    public int Level { get; set; }
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = new();
}