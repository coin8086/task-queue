namespace Rz.TaskQueue;

public interface IMessageQueue
{
    public void Enqueue(string messsage, int? lease = null, string? queue = null);

    public IQueueMessage Dequeue(string? queue = null);
}
