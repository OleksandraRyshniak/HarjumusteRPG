namespace HarjumusteRPG.API.Models;

// A pet the user has bought in the shop. Cat and dog are free and not stored here.
public class UserPet
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string PetKey { get; set; } = string.Empty;
    public DateTime BoughtAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}