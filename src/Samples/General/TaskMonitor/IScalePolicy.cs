using Rz.TaskQueue.Client;

namespace Rz.TaskQueueMonitor;

public interface IScalePolicy
{
    int SampleInterval { get; } //In seconds

    void Sample(QueueStat stat);

    int? Suggest();
}
