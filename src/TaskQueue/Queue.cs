using Microsoft.EntityFrameworkCore;

namespace Rz.TaskQueue;

public class Queue : IQueue
{
    public Queue(IDbContextFactory<PsqlContext> psqlContextFactory, string name, int messageLease = 60)
    {
        throw new NotImplementedException();
    }

    public string Name => throw new NotImplementedException();

    public int MessageLease => throw new NotImplementedException();

    public void Create()
    {
        throw new NotImplementedException();
    }

    public void Delete()
    {
        throw new NotImplementedException();
    }

    public void PutMessage(string messsage, int? lease = null)
    {
        throw new NotImplementedException();
    }

    public IQueueMessage? GetMessage()
    {
        throw new NotImplementedException();
    }

    public void ExtendMessageLease(int messageId, string receipt, int? lease = null)
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
