
using Microsoft.Extensions.Options;
using Rz.TaskQueue.Client;
using System.ComponentModel.DataAnnotations;

namespace TaskMonitor;

public class MonitorOptions
{
    [Required]
    public string Queue { get; set; } = default!;

    public int Interval { get; set; } = 15; //In seconds
}

public class Monitor : BackgroundService
{
    private readonly ILogger<Monitor> _logger;

    private readonly MonitorOptions _options;

    private readonly IQueueClient _client;

    private readonly IScalePolicy _policy;

    private readonly IWorkerProvider _provider;

    public Monitor(ILogger<Monitor> logger, IOptions<MonitorOptions> options, IQueueClient client, IScalePolicy policy, IWorkerProvider provider)
    {
        _logger = logger;
        _options = options.Value;
        _client = client;
        _policy = policy;
        _provider = provider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
#pragma warning disable CS4014
        //Start sampler in another thread.
        Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //TODO: Retry on HTTP error
                    var stat = await _client.GetQueueStatAsync(_options.Queue, stoppingToken);
                    _logger.LogDebug("Queue stat: {stat}", stat);

                    _policy.Sample(stat);

                    await Task.Delay(_policy.SampleInterval * 1000, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Sampling is canceled.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error when sampling.");
                    throw;
                }
            }
        }, stoppingToken);
#pragma warning restore CS4014

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var target = _policy.Suggest();
                _logger.LogInformation("Policy suggests: {target}", target);

                if (target != null)
                {
                    var result = await _provider.ProvideAsync(_options.Queue, target.Value, stoppingToken);
                    _logger.LogInformation("Provider result: {result}", result);
                }

                await Task.Delay(_options.Interval * 1000, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Monitor is canceled.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in monitor.");
                throw;
            }
        }
    }
}

public static partial class IServiceCollectionExtensions
{
    public static IServiceCollection AddMonitor(this IServiceCollection services)
    {
        services.AddHostedService<Monitor>();
        services.AddOptionsWithValidateOnStart<MonitorOptions>()
            .BindConfiguration("Monitor")
            .ValidateDataAnnotations();

        return services;
    }
}
