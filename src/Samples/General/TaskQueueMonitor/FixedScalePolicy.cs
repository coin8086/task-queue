using Microsoft.Extensions.Options;
using Rz.TaskQueue.Client;
using System.ComponentModel.DataAnnotations;

namespace Rz.TaskQueueMonitor;

public class FixedScalePolicyOptions
{
    [Range(1, int.MaxValue)]
    public int SampleInterval { get; set; } = 2;    //In seconds

    [Range(1, int.MaxValue)]
    public int Target { get; set; } = 5;

    [Range(1, int.MaxValue)]
    public int ScaleInLookBack { get; set; } = 10;

    [Range(1, int.MaxValue)]
    public int ScaleOutLookBack { get; set; } = 5;
}

public class FixedScalePolicy : IScalePolicy
{
    private readonly int _historySize;

    private readonly Queue<QueueStat> _history;

    private readonly FixedScalePolicyOptions _options;

    private readonly object _locker = new object();

    public FixedScalePolicy(IOptions<FixedScalePolicyOptions> options)
    {
        _options = options.Value;
        _historySize = int.Max(_options.ScaleInLookBack, _options.ScaleOutLookBack);
        _history = new Queue<QueueStat>(_historySize);
    }

    public int SampleInterval => _options.SampleInterval;

    public void Sample(QueueStat stat)
    {
        lock (_locker)
        {
            if (_history.Count == _historySize)
            {
                _history.Dequeue();
            }
            _history.Enqueue(stat);
        }
    }

    public int? Suggest()
    {
        lock (_locker)
        {
            //Should it scale out?
            if (_history.Count >= _options.ScaleOutLookBack)
            {
                //Get the latest history for scale out
                var history = _history.Skip(_history.Count - _options.ScaleOutLookBack);
                var shouldScaleOut = true;
                foreach (var item in history)
                {
                    if (item.MessageTotal == 0)
                    {
                        //If there's already a worker on queue, then do not scale out.
                        shouldScaleOut = false;
                        break;
                    }
                }
                if (shouldScaleOut)
                {
                    return _options.Target;
                }
            }

            //Should it scale in?
            if (_history.Count >= _options.ScaleInLookBack)
            {
                //Get the latest history for scale in
                var history = _history.Skip(_history.Count - _options.ScaleInLookBack);
                var shouldScaleIn = true;
                foreach (var item in history)
                {
                    if (item.MessageTotal != 0)
                    {
                        shouldScaleIn = false;
                        break;
                    }
                }
                if (shouldScaleIn)
                {
                    return 0;
                }
            }

            //No suggestion.
            return null;
        }
    }
}

public static partial class IServiceCollectionExtensions
{
    public static IServiceCollection AddFixedScalePolicy(this IServiceCollection services)
    {
        services.AddSingleton<IScalePolicy, FixedScalePolicy>();
        services.AddOptionsWithValidateOnStart<FixedScalePolicyOptions>()
            .BindConfiguration("FixedScalePolicy")
            .ValidateDataAnnotations();

        return services;
    }
}