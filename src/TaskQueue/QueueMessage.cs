
namespace Rz.TaskQueue;

internal class QueueMessage : IQueueMessage
{
    public int Id => throw new NotImplementedException();

    public string Receipt => throw new NotImplementedException();

    public string Queue => throw new NotImplementedException();

    public string Content => throw new NotImplementedException();

    public int RequeueCount => throw new NotImplementedException();

    public DateTimeOffset CreatedAt => throw new NotImplementedException();

    public DateTimeOffset LeaseExpiredAt => throw new NotImplementedException();
}
