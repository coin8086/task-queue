namespace Rz.TaskQueue;

public interface IQueueStat
{
    string Queue { get; }

    int MessageTotal { get; }

    int MessageAvailable { get; }
}
