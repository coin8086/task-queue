namespace Rz.TaskQueue;

public interface IQueueMessage
{
    public string Queue { get; }

    public string Content { get; }

    public int RequeueCount { get; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset LeaseExpiredAt { get; }

    public void ExtendLease(int lease);

    public void DeleteFromQueue();

    public void ReturnToQueue();
}
