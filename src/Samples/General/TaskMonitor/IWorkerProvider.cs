namespace Rz.TaskQueueMonitor;

public interface IWorkerProvider
{
    Task<int?> ProvideAsync(string queue, int target, CancellationToken token = default);
}
