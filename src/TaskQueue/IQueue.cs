namespace Rz.TaskQueue;

public interface IQueue
{
    public void Enqueue(string messsage, int? lease = null);

    public IQueueMessage? Dequeue();

    public void ExtendMessageLease(int messageId, string receipt, int lease);

    public void DeleteMessage(int messageId, string receipt);

    public void ReturnMessage(int messageId, string receipt);
}
