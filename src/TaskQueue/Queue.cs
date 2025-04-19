using Microsoft.EntityFrameworkCore;

namespace Rz.TaskQueue;

public class Queue : IQueue
{
    private readonly IDbContextFactory<PsqlContext> _dbContextFactory;

    private readonly string _name;

    private readonly int _messageLease;

    public Queue(IDbContextFactory<PsqlContext> psqlContextFactory, string name, int messageLease = 60)
    {
        _dbContextFactory = psqlContextFactory;
        _name = name;
        _messageLease = messageLease;
    }

    public string Name => _name;

    public int MessageLease => _messageLease;

    public void Create()
    {
        throw new NotImplementedException();
    }

    public void Delete()
    {
        throw new NotImplementedException();
    }

    public void PutMessage(string messsage)
    {
        throw new NotImplementedException();
    }

    public IQueueMessage? GetMessage(int? lease = null)
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
