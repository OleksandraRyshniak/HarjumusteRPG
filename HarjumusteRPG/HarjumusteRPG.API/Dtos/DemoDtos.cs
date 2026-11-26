namespace HarjumusteRPG.API.Dtos;

public class DemoTimeDto
{
    public DateOnly Today { get; set; }
    public int DaysAhead { get; set; }
    public int StreaksReset { get; set; }
}