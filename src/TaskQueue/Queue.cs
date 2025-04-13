namespace Rz.TaskQueue;

internal class Queue : IQueue
{
    public void Enqueue(string messsage, int? lease = null)
    {
        throw new NotImplementedException();
    }

    public IQueueMessage? Dequeue()
    {
        throw new NotImplementedException();
    }

    public void ExtendMessageLease(int messageId, string receipt, int lease)
    {
        throw new NotImplementedException();
    }

    public void DeleteMessage(int messageId, string receipt)
    {
        throw new NotImplementedException();
    }

    public void ReturnMessage(int messageId, string receipt)
    {
        throw new NotImplementedException();
    }
}
