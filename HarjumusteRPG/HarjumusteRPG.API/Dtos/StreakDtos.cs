namespace HarjumusteRPG.API.Dtos
{
    public class StreakResetDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public int PreviousStreak { get; set; }
    }
}
