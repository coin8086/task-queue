namespace Rz.TaskQueue;

public interface IQueueMessage
{
    int Id { get; }

    string Receipt { get; }

    string Queue { get; }

    string Content { get; }

    int RequeueCount { get; }

    DateTimeOffset CreatedAt { get; }

    DateTimeOffset LeaseExpiredAt { get; }
}
