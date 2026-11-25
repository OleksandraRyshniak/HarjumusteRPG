namespace HarjumusteRPG.Worker;

public class WorkerOptions
{
    public string BaseUrl { get; set; } = "http://localhost:5086/";
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    /// <summary>Kellaaeg (HH:mm), millal seeriad kontrollitakse.</summary>
    public string RunAt { get; set; } = "00:05";
}