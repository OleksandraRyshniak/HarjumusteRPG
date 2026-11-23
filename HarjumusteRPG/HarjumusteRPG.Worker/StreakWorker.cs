using Microsoft.Extensions.Options;

namespace HarjumusteRPG.Worker;

public class StreakWorker : BackgroundService
{
    private readonly StreakApiClient _api;
    private readonly WorkerOptions _options;
    private readonly ILogger<StreakWorker> _logger;

    public StreakWorker(StreakApiClient api, IOptions<WorkerOptions> options, ILogger<StreakWorker> logger)
    {
        _api = api;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var runAt = TimeSpan.Parse(_options.RunAt);
        DateOnly? lastRun = null;

        _logger.LogInformation("Worker käivitus, seeriate kontroll iga päev kell {RunAt}", _options.RunAt);

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        do
        {
            var now = DateTime.Now;
            var today = DateOnly.FromDateTime(now);

            if (now.TimeOfDay >= runAt && lastRun != today)
            {
                try
                {
                    var reset = await _api.CheckStreaksAsync(stoppingToken);
                    lastRun = today;

                    _logger.LogInformation("Seeriate kontroll tehtud, katkestati {Count} seeriat", reset.Count);
                    foreach (var r in reset)
                        _logger.LogInformation("  {User}: seeria {Streak} katkes", r.Username, r.PreviousStreak);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // lastRun jääb tühjaks, järgmine katse on minuti pärast
                    _logger.LogError(ex, "Seeriate kontroll ebaõnnestus");
                }
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}