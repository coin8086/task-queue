namespace Rz.TaskQueue;

internal class QueueStat : IQueueStat
{
    public string Queue { get; set; } = default!;

    public int MessageTotal {  get; set; }

    public int MessageAvailable {  get; set; }
}
