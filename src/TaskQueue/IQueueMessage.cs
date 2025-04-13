namespace Rz.TaskQueue;

public interface IQueueMessage
{
    public int Id { get; }

    public string Receipt { get; }

    public string Queue { get; }

    public string Content { get; }

    public int RequeueCount { get; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset LeaseExpiredAt { get; }
}
