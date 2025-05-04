namespace Rz.TaskQueue.Client;

public class InvalidQueueOperation : Exception
{
}

public interface IQueueClient
{
    Task CreateQueueAsync(string queue, CancellationToken token = default);

    Task DeleteQueueAsync(string queue, CancellationToken token = default);

    Task<QueueStat> GetQueueStatAsync(string queue, CancellationToken token = default);

    Task PutMessageAsync(string queue, string message, CancellationToken token = default);

    Task<QueueMessage?> GetMessageAsync(string queue, int? lease = null, CancellationToken token = default);

    Task ExtendMessageLeaseAsync(string queue, int messageId, string receipt, int? lease = null, CancellationToken token = default);

    Task ReturnMessageAsync(string queue, int messageId, string receipt, CancellationToken token = default);

    Task DeleteMessageAsync(string queue, int messageId, string receipt, CancellationToken token = default);
}
