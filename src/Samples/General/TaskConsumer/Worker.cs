using Microsoft.Extensions.Options;
using Rz.TaskQueue.Client;
using System.ComponentModel.DataAnnotations;

namespace TaskConsumer;

public class WorkerOptions
{
    [Required]
    public string Queue { get; set; } = default!;

    public int Lease { get; set; } = 10;    //In seconds

    public int ProcessTime { get; set; } = 1000;    //In milliseconds
}

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    private readonly IQueueClient _client;

    private readonly WorkerOptions _options;

    public Worker(ILogger<Worker> logger, IQueueClient client, IOptions<WorkerOptions> options)
    {
        _logger = logger;
        _client = client;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await WorkAsync(stoppingToken);
        }
    }

    private async Task WorkAsync(CancellationToken stoppingToken)
    {
        var lease = _options.Lease * 1000;
        var interval = (int)(lease * 3.0 / 4);

        while (!stoppingToken.IsCancellationRequested)
        {
            QueueMessage message;
            try
            {
                //TODO: Retry on HTTP error.
                message = await _client.WaitMessageAsync(_options.Queue, _options.Lease, token: stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation($"{nameof(Worker)} is canceled.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when waiting for message.");
                throw;
            }

            _logger.LogDebug("Received message: '{msg}'", message.Content);

            try
            {
                using var timer = new Timer(async _ => {
                    try
                    {
                        await _client.ExtendMessageLeaseAsync(_options.Queue, message.Id, message.Receipt, _options.Lease);
                    }
                    catch (Exception ex)
                    {
                        //TODO: Retry after a short backoff since the lease has passed 3/4!
                        _logger.LogWarning(ex, "Failed in extending lease of message {id}.", message.Id);
                    }
                }, null, interval, interval);

                try
                {
                    await ProcessMessageAsync(message, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation($"{nameof(Worker)} is canceled.");
                    timer.Change(Timeout.Infinite, Timeout.Infinite);
                    await _client.ReturnMessageAsync(_options.Queue, message.Id, message.Receipt);
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error when processing message.");
                    timer.Change(Timeout.Infinite, Timeout.Infinite);
                    await _client.ReturnMessageAsync(_options.Queue, message.Id, message.Receipt);
                    throw;
                }

                await _client.DeleteMessageAsync(_options.Queue, message.Id, message.Receipt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(WorkAsync)} error.");
                throw;
            }
        }

        if (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Cancellation is requested. Quit.");
        }
    }

    private async Task ProcessMessageAsync(QueueMessage message, CancellationToken stoppingToken)
    {
        //Process the message ...
        await Task.Delay(_options.ProcessTime, stoppingToken);
    }
}

static class ServiceCollectionWorkerServiceExtensions
{
    public static IServiceCollection AddWorkerService(this IServiceCollection services)
    {
        services.AddHostedService<Worker>();
        services.AddOptionsWithValidateOnStart<WorkerOptions>()
            .BindConfiguration("Worker")
            .ValidateDataAnnotations();
        return services;
    }
}
