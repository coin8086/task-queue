
using Microsoft.Extensions.Options;
using Rz.TaskQueue.Client;
using System.ComponentModel.DataAnnotations;

namespace TaskMonitor;

public class MonitorOptions
{
    [Required]
    public string Queue { get; set; } = default!;

    public int QueryInterval { get; set; }  //In seconds
}

public class Monitor : BackgroundService
{
    private readonly ILogger<Monitor> _logger;

    private readonly MonitorOptions _options;

    private readonly IQueueClient _client;

    public Monitor(ILogger<Monitor> logger, IOptions<MonitorOptions> options, IQueueClient client)
    {
        _logger = logger;
        _options = options.Value;
        _client = client;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }
}

static class MonitorIServiceCollectionExtensions
{
    public static IServiceCollection AddWorkerService(this IServiceCollection services)
    {
        services.AddHostedService<Monitor>();
        services.AddOptionsWithValidateOnStart<MonitorOptions>()
            .BindConfiguration("Monitor")
            .ValidateDataAnnotations();
        return services;
    }
}
