namespace Rz.TaskQueue.Client;

public class InvalidQueueOperation : Exception
{
}

public interface IQueueClient
{
    Task CreateQueueAsync(string queue);

    Task DeleteQueueAsync(string queue);

    Task<QueueStat> GetQueueStatAsync(string queue);

    Task PutMessageAsync(string queue, string message);

    Task<QueueMessage?> GetMessageAsync(string queue, int? lease = null);

    Task ExtendMessageLeaseAsync(string queue, int messageId, string receipt, int? lease = null);

    Task ReturnMessageAsync(string queue, int messageId, string receipt);

    Task DeleteMessageAsync(string queue, int messageId, string receipt);
}
