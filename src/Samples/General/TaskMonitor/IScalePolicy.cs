using Rz.TaskQueue.Client;

namespace TaskMonitor;

public interface IScalePolicy
{
    int SampleInterval { get; } //In seconds

    void Sample(QueueStat stat);

    int? Suggest();
}
