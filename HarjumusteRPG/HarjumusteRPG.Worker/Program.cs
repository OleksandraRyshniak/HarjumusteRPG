using HarjumusteRPG.Worker;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<WorkerOptions>(builder.Configuration.GetSection("Worker"));

builder.Services.AddHttpClient<StreakApiClient>((sp, http) =>
{
    var options = sp.GetRequiredService<IOptions<WorkerOptions>>().Value;
    http.BaseAddress = new Uri(options.BaseUrl);
});

builder.Services.AddHostedService<StreakWorker>();

builder.Build().Run();