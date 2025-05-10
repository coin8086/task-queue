
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Rz.TaskQueueMonitor;

public class ProcessWorkerProviderOptions
{
    [Required]
    public string Command { get; set; } = default!;

    public string? Arguments { get; set; }
}

public class ProcessWorkerProvider : IWorkerProvider
{
    private readonly ILogger _logger;

    private readonly ProcessWorkerProviderOptions _options;

    private readonly ProcessStartInfo _startInfo;

    private readonly string _processName;

    public ProcessWorkerProvider(ILogger<ProcessWorkerProvider> logger, IOptions<ProcessWorkerProviderOptions> options)
    {
        _logger = logger;
        _options = options.Value;
        _startInfo = new ProcessStartInfo()
        {
            FileName = _options.Command,
            Arguments = _options.Arguments,
            UseShellExecute = false,
        };
        _processName = Path.GetFileNameWithoutExtension(_options.Command);
        ArgumentNullException.ThrowIfNull(_processName);
    }

    public async Task<int?> ProvideAsync(string queue, int target, CancellationToken token = default)
    {
        var count = CountWorker();
        _logger.LogInformation("Queue={queue}, Current={current}, Target={target}", queue, count, target);

        if (count == target)
        {
            return target;
        }

        if (count > target)
        {
            var down = count - target;
            _logger.LogInformation("Kill {num} worker(s).", down);

            var killed = await KillWorkersAsync(count - target, token).ConfigureAwait(false);
            _logger.LogInformation("Killed {num} worker(s).", killed);
            return count - killed;
        }

        {
            Debug.Assert(count < target);
            var up = target - count;
            var tasks = new Task[up];
            _logger.LogInformation("Start {num} worker(s).", up);

            for (var i = 0; i < up; i++)
            {
                tasks[i] = StartWorkerAsync(token);
            }

            try
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error when starting worker process.");
            }

            var started = tasks.Count(t => t.IsCompletedSuccessfully);
            _logger.LogInformation("Started {num} worker(s).", started);
            return count + started;
        }
    }

    private int CountWorker()
    {
        var processes = Process.GetProcessesByName(_processName);
        return processes.Length;
    }

    private Task<int> KillWorkersAsync(int num, CancellationToken token = default)
    {
        Debug.Assert(num >= 0);

        try
        {
            var processes = Process.GetProcessesByName(_processName);
            var count = 0;
            foreach (var process in processes)
            {
                token.ThrowIfCancellationRequested();

                if (count == num)
                {
                    break;
                }

                try
                {
                    process.Kill(true);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error when killing process {id}", process.Id);
                    continue;
                }
                count++;
            }
            return Task.FromResult(count);
        }
        catch (Exception ex)
        {
            return (Task<int>)Task.FromException(ex);
        }
    }

    private Task StartWorkerAsync(CancellationToken token = default)
    {
        try
        {
            using var process = new Process()
            {
                StartInfo = _startInfo,

                //NOTE: This is to avoid the parent process being zombie in some situation. See 
                //https://github.com/dotnet/runtime/issues/21661
                EnableRaisingEvents = true,
            };

            token.ThrowIfCancellationRequested();
            process.Start();
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
        return Task.CompletedTask;
    }
}

public static partial class IServiceCollectionExtensions
{
    public static IServiceCollection AddProcessWorkerProvider(this IServiceCollection services)
    {
        services.AddSingleton<IWorkerProvider, ProcessWorkerProvider>();
        services.AddOptionsWithValidateOnStart<ProcessWorkerProviderOptions>()
            .BindConfiguration("ProcessWorkerProvider")
            .ValidateDataAnnotations();

        return services;
    }
}
