using Microsoft.EntityFrameworkCore;
using Rz.TaskQueue.Models;

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
        return Task.CompletedTask;
    }

    public async Task DeleteAsync()
    {
        using var db = _dbContextFactory.CreateDbContext();
        await db.Messages.Where(msg => msg.Queue == Name).ExecuteDeleteAsync().ConfigureAwait(false);
    }

    public async Task PutMessageAsync(string messsage)
    {
        var msg = new Message()
        {
            Queue = Name,
            Content = messsage,
            RequeueCount = 0,
            CreatedAt = DateTimeOffset.UtcNow,  //TODO: Configure db model to generate this field automatically?
        };
        using var db = _dbContextFactory.CreateDbContext();
        db.Messages.Add(msg);
        await db.SaveChangesAsync().ConfigureAwait(false);
    }

    //TODO: Add a test for concurrent calls.
    public async Task<IQueueMessage?> GetMessageAsync(int? lease = null)
    {
        if (lease != null && lease <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lease), "The message lease time must be greater than 0.");
        }

        using var db = _dbContextFactory.CreateDbContext();
        await using var transaction = await db.Database.BeginTransactionAsync().ConfigureAwait(false);

        var now = DateTimeOffset.UtcNow;

        //TODO: What should the indexes look like for the query?
        FormattableString sql = $"""
        select * from "Messages" where "Queue" = {Name} and ("LeaseExpiredAt" is null or "LeaseExpiredAt" < {now})
        order by "CreatedAt" asc limit 1
        for update
        """;

        var msg = await db.Messages.FromSql(sql).FirstOrDefaultAsync().ConfigureAwait(false);
        if (msg == null)
        {
            return null;
        }

        if (msg.LeaseExpiredAt != null)
        {
            msg.RequeueCount++;
        }

        msg.Receipt = Guid.NewGuid().ToString();
        msg.LeaseExpiredAt = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(lease ?? MessageLease);
        await db.SaveChangesAsync().ConfigureAwait(false);
        await transaction.CommitAsync().ConfigureAwait(false);

        return new QueueMessage()
        {
            Id = msg.Id,
            Receipt = msg.Receipt,
            Queue = Name,
            Content = msg.Content,
            RequeueCount = msg.RequeueCount,
            CreatedAt = msg.CreatedAt,
            LeaseExpiredAt = msg.LeaseExpiredAt.Value,
        };
    }

    public async Task ExtendMessageLeaseAsync(int messageId, string receipt, int? lease = null)
    {
        using var db = _dbContextFactory.CreateDbContext();
        var now = DateTimeOffset.UtcNow;
        var count = await db.Messages.Where(msg => msg.Id == messageId && msg.Receipt == receipt && msg.Queue == Name && msg.LeaseExpiredAt > now)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(msg => msg.LeaseExpiredAt, msg => msg.LeaseExpiredAt + TimeSpan.FromSeconds(lease ?? MessageLease))
            ).ConfigureAwait(false);

        if (count != 1)
        {
            throw new InvalidQueueOperation();
        }
    }

    public async Task DeleteMessageAsync(int messageId, string receipt)
    {
        using var db = _dbContextFactory.CreateDbContext();
        var now = DateTimeOffset.UtcNow;
        var count = await db.Messages.Where(msg => msg.Id == messageId && msg.Receipt == receipt && msg.Queue == Name && msg.LeaseExpiredAt > now)
            .ExecuteDeleteAsync().ConfigureAwait(false);

        if (count != 1)
        {
            throw new InvalidQueueOperation();
        }
    }

    public async Task ReturnMessageAsync(int messageId, string receipt)
    {
        using var db = _dbContextFactory.CreateDbContext();
        var now = DateTimeOffset.UtcNow;
        var count = await db.Messages.Where(msg => msg.Id == messageId && msg.Receipt == receipt && msg.Queue == Name && msg.LeaseExpiredAt > now)
            .ExecuteUpdateAsync(setters =>
                setters
                    .SetProperty(msg => msg.Receipt, msg => null)
                    .SetProperty(msg => msg.LeaseExpiredAt, msg => null)
                    .SetProperty(msg => msg.RequeueCount, msg => msg.RequeueCount + 1)
            ).ConfigureAwait(false);

        if (count != 1)
        {
            throw new InvalidQueueOperation();
        }
    }

    public async Task<IQueueStat> GetStatAsync()
    {
        using var db = _dbContextFactory.CreateDbContext();
        //TODO: Is this correct/proper to make two queries in a transaction?
        await using var transaction = await db.Database.BeginTransactionAsync().ConfigureAwait(false);
        var now = DateTimeOffset.UtcNow;
        var total = await db.Messages.Where(msg => msg.Queue == Name).CountAsync();
        var available = await db.Messages.Where(msg => msg.Queue == Name && (msg.LeaseExpiredAt == null || msg.LeaseExpiredAt <= now)).CountAsync();
        await transaction.CommitAsync().ConfigureAwait(false);

        return new QueueStat { Queue = Name, MessageTotal = total, MessageAvailable = available };
    }
}
