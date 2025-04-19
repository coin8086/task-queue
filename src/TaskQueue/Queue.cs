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

    public Task CreateAsync()
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync()
    {
        throw new NotImplementedException();
    }

    public Task PutMessageAsync(string messsage)
    {
        throw new NotImplementedException();
    }

    public Task<IQueueMessage?> GetMessageAsync(int? lease = null)
    {
        throw new NotImplementedException();
    }

    public Task ExtendMessageLeaseAsync(int messageId, string receipt, int? lease = null)
    {
        throw new NotImplementedException();
    }

    public Task DeleteMessageAsync(int messageId, string receipt)
    {
        throw new NotImplementedException();
    }

    public Task ReturnMessageAsync(int messageId, string receipt)
    {
        throw new NotImplementedException();
    }
}
