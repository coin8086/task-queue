using Xunit.Abstractions;

namespace Rz.TaskQueue.Test;

public class QueueTest : DbTest
{
    private const int DefaultMessageLease = 60;

    private const string Letters = "abcdefghijklmnopqrstuvwxyz0123456789";

    public QueueTest(ITestOutputHelper testout, TestDatabaseFixture fixture) : base(testout, fixture)
    {
    }

    private static string RandomQueueName => $"queue_{new string(Random.Shared.GetItems<char>(Letters, 6))}";

    private async Task TestQueueAsync(Func<IQueue, Task> test)
    {
        IQueue queue = new Queue(DbContextFactory, RandomQueueName, DefaultMessageLease);
        await queue.CreateAsync();
        try
        {
            await test(queue);
        }
        finally
        {
            await queue.DeleteAsync();
        }
    }

    private static void AssertMessageLease(int lease, DateTimeOffset leaseExpiredAt, DateTimeOffset? now = null)
    {
        var timeLeft = leaseExpiredAt - (now ?? DateTimeOffset.Now);
        Assert.True(timeLeft.TotalSeconds > 0 && timeLeft.TotalSeconds <= lease);
    }

    [Fact]
    public async Task TestCreateAndDeleteQueue()
    {
        IQueue queue = new Queue(DbContextFactory, RandomQueueName, DefaultMessageLease);

        await queue.CreateAsync();
        try
        {
            var messages = new string[] { "1", "2", "3" };
            foreach (var message in messages)
            {
                await queue.PutMessageAsync(message);
            }

            var msg = await queue.GetMessageAsync();
            Assert.NotNull(msg);

            await queue.DeleteAsync();

            //After queue is deleted, no message can be retrieved from it.
            var msg2 = await queue.GetMessageAsync();
            Assert.Null(msg2);

            //And any operations on the retrieved msg should fail.
            await Assert.ThrowsAsync<InvalidQueueOperation>(() =>
            {
                return queue.ExtendMessageLeaseAsync(msg.Id, msg.Receipt, queue.MessageLease);
            });

            await Assert.ThrowsAsync<InvalidQueueOperation>(() =>
            {
                return queue.ReturnMessageAsync(msg.Id, msg.Receipt);
            });

            await Assert.ThrowsAsync<InvalidQueueOperation>(() =>
            {
                return queue.DeleteMessageAsync(msg.Id, msg.Receipt);
            });
        }
        finally
        {
            await queue.DeleteAsync();
        }
    }

    [Fact]
    public async Task TestEnqueueAndDequeue()
    {
        await TestQueueAsync(async queue =>
        {
            var messages = new string[] { "1", "2", "3" };
            foreach (var message in messages)
            {
                await queue.PutMessageAsync(message);
            }

            for (var i = 0; i < messages.Length; i++)
            {
                var msg = await queue.GetMessageAsync();
                Assert.NotNull(msg);
                Assert.Equal(queue.Name, msg.Queue);
                Assert.Equal(messages[i], msg.Content);
                Assert.Equal(0, msg.RequeueCount);
                AssertMessageLease(queue.MessageLease, msg.LeaseExpiredAt);
            }

            //When no visible/available message in queue, null is returned.
            var msg2 = await queue.GetMessageAsync();
            Assert.Null(msg2);
        });
    }

    [Fact]
    public async Task TestMessageLeaseTimeout()
    {
        await TestQueueAsync(async queue =>
        {
            var msgContent = "abc";
            var lease = 2;
            await queue.PutMessageAsync(msgContent);

            var msg = await queue.GetMessageAsync(lease);
            Assert.NotNull(msg);
            Assert.Equal(msgContent, msg.Content);
            Assert.Equal(0, msg.RequeueCount);
            AssertMessageLease(lease, msg.LeaseExpiredAt);

            var msg2 = await queue.GetMessageAsync(lease);
            Assert.Null(msg2);

            //Pass half timeLeft of lease
            await Task.Delay((int)(lease / 2.0 * 1000));

            //The lease of msg is not expired.
            msg2 = await queue.GetMessageAsync(lease);
            Assert.Null(msg2);

            //Pass another half timeLeft of lease
            await Task.Delay((int)(lease / 2.0 * 1000));

            //The lease of msg is expired.
            //The the same message is available in queue again.
            msg2 = await queue.GetMessageAsync(lease);
            Assert.NotNull(msg2);
            Assert.Equal(msg.Id, msg2.Id);
            Assert.NotEqual(msg.Receipt, msg2.Receipt);
            Assert.Equal(1, msg2.RequeueCount);
            AssertMessageLease(lease, msg2.LeaseExpiredAt);
        });
    }

    [Fact]
    public async Task TestMessageLeaseExtension()
    {
        await TestQueueAsync(async queue =>
        {
            var msgContent = "abc";
            var lease = 2;
            await queue.PutMessageAsync(msgContent);

            var msg = await queue.GetMessageAsync(lease);
            Assert.NotNull(msg);
            Assert.Equal(msgContent, msg.Content);
            Assert.Equal(0, msg.RequeueCount);
            AssertMessageLease(lease, msg.LeaseExpiredAt);

            var msg2 = await queue.GetMessageAsync(lease);
            Assert.Null(msg2);

            //Pass half timeLeft of lease
            await Task.Delay((int)(lease / 2.0 * 1000));

            //The lease of msg is not expired.
            msg2 = await queue.GetMessageAsync(lease);
            Assert.Null(msg2);

            await queue.ExtendMessageLeaseAsync(msg.Id, msg.Receipt, lease);

            //Pass another half timeLeft of lease
            await Task.Delay((int)(lease / 2.0 * 1000));

            //The extended lease of msg is not expired.
            msg2 = await queue.GetMessageAsync(lease);
            Assert.Null(msg2);

            //Pass a third half timeLeft of lease
            await Task.Delay((int)(lease / 2.0 * 1000));

            //The extended lease of msg is not expired.
            msg2 = await queue.GetMessageAsync(lease);
            Assert.Null(msg2);

            //Pass a forth half timeLeft of lease
            await Task.Delay((int)(lease / 2.0 * 1000));

            //The extended lease of msg is expired.
            msg2 = await queue.GetMessageAsync(lease);
            Assert.NotNull(msg2);
            Assert.Equal(msg.Id, msg2.Id);
            Assert.NotEqual(msg.Receipt, msg2.Receipt);
            Assert.Equal(1, msg2.RequeueCount);
            AssertMessageLease(lease, msg2.LeaseExpiredAt);
        });
    }

    [Fact]
    public async Task TestMessageLeaseExtensionAfterTimeout()
    {
        await TestQueueAsync(async queue =>
        {
            var msgContent = "abc";
            var lease = 2;
            await queue.PutMessageAsync(msgContent);

            var msg = await queue.GetMessageAsync(lease);
            Assert.NotNull(msg);
            Assert.Equal(msgContent, msg.Content);
            Assert.Equal(0, msg.RequeueCount);
            AssertMessageLease(lease, msg.LeaseExpiredAt);

            var msg2 = await queue.GetMessageAsync(lease);
            Assert.Null(msg2);

            //Pass half timeLeft of lease
            await Task.Delay((int)(lease / 2.0 * 1000));

            //The lease of msg is not expired.
            msg2 = await queue.GetMessageAsync(lease);
            Assert.Null(msg2);

            //Pass another half timeLeft of lease
            await Task.Delay((int)(lease / 2.0 * 1000));

            //The lease of msg is expired.
            await Assert.ThrowsAsync<InvalidQueueOperation>(() =>
            {
                return queue.ExtendMessageLeaseAsync(msg.Id, msg.Receipt, lease);
            });

            //The the same message is available in queue again.
            msg2 = await queue.GetMessageAsync(lease);
            Assert.NotNull(msg2);
            Assert.Equal(msg.Id, msg2.Id);
            Assert.NotEqual(msg.Receipt, msg2.Receipt);
            Assert.Equal(1, msg2.RequeueCount);
            AssertMessageLease(lease, msg2.LeaseExpiredAt);
        });
    }

    [Fact]
    public async Task TestMessageDeletion()
    {
        await TestQueueAsync(async queue =>
        {
            var msgContent = "abc";
            var lease = 2;
            await queue.PutMessageAsync(msgContent);

            var msg = await queue.GetMessageAsync(lease);
            Assert.NotNull(msg);
            Assert.Equal(msgContent, msg.Content);
            Assert.Equal(0, msg.RequeueCount);
            AssertMessageLease(lease, msg.LeaseExpiredAt);

            var msg2 = await queue.GetMessageAsync(lease);
            Assert.Null(msg2);

            //Pass half timeLeft of lease
            await Task.Delay((int)(lease / 2.0 * 1000));

            //The lease of msg is not expired.
            msg2 = await queue.GetMessageAsync(lease);
            Assert.Null(msg2);

            await queue.DeleteMessageAsync(msg.Id, msg.Receipt);

            //Pass another half timeLeft of lease
            await Task.Delay((int)(lease / 2.0 * 1000));

            //No more message in the queue.
            msg2 = await queue.GetMessageAsync(lease);
            Assert.Null(msg2);
        });
    }

    [Fact]
    public async Task TestMessageDeletionAfterTimeout()
    {
        await TestQueueAsync(async queue =>
        {
            var msgContent = "abc";
            var lease = 2;
            await queue.PutMessageAsync(msgContent);

            var msg = await queue.GetMessageAsync(lease);
            Assert.NotNull(msg);
            Assert.Equal(msgContent, msg.Content);
            Assert.Equal(0, msg.RequeueCount);
            AssertMessageLease(lease, msg.LeaseExpiredAt);

            var msg2 = await queue.GetMessageAsync(lease);
            Assert.Null(msg2);

            //Pass half timeLeft of lease
            await Task.Delay((int)(lease / 2.0 * 1000));

            //The lease of msg is not expired.
            msg2 = await queue.GetMessageAsync(lease);
            Assert.Null(msg2);

            //Pass another half timeLeft of lease
            await Task.Delay((int)(lease / 2.0 * 1000));

            //The lease of msg is expired.
            await Assert.ThrowsAsync<InvalidQueueOperation>(() =>
            {
                return queue.DeleteMessageAsync(msg.Id, msg.Receipt);
            });

            //The the same message is available in queue again.
            msg2 = await queue.GetMessageAsync(lease);
            Assert.NotNull(msg2);
            Assert.Equal(msg.Id, msg2.Id);
            Assert.NotEqual(msg.Receipt, msg2.Receipt);
            Assert.Equal(1, msg2.RequeueCount);
            AssertMessageLease(lease, msg2.LeaseExpiredAt);
        });
    }

    [Fact]
    public async Task TestMessageReturn()
    {
        await TestQueueAsync(async queue =>
        {
            var msgContent = "abc";
            var lease = 2;
            await queue.PutMessageAsync(msgContent);

            var msg = await queue.GetMessageAsync(lease);
            Assert.NotNull(msg);
            Assert.Equal(msgContent, msg.Content);
            Assert.Equal(0, msg.RequeueCount);
            AssertMessageLease(lease, msg.LeaseExpiredAt);

            var msg2 = await queue.GetMessageAsync(lease);
            Assert.Null(msg2);

            //Pass half timeLeft of lease
            await Task.Delay((int)(lease / 2.0 * 1000));

            //The lease of msg is not expired.
            msg2 = await queue.GetMessageAsync(lease);
            Assert.Null(msg2);

            await queue.ReturnMessageAsync(msg.Id, msg.Receipt);

            //The the same message is available in queue again.
            msg2 = await queue.GetMessageAsync(lease);
            Assert.NotNull(msg2);
            Assert.Equal(msg.Id, msg2.Id);
            Assert.NotEqual(msg.Receipt, msg2.Receipt);
            Assert.Equal(1, msg2.RequeueCount);
            AssertMessageLease(lease, msg2.LeaseExpiredAt);
        });
    }

    [Fact]
    public async Task TestMessageReturnAfterTimeout()
    {
        await TestQueueAsync(async queue =>
        {
            var msgContent = "abc";
            var lease = 2;
            await queue.PutMessageAsync(msgContent);

            var msg = await queue.GetMessageAsync(lease);
            Assert.NotNull(msg);
            Assert.Equal(msgContent, msg.Content);
            Assert.Equal(0, msg.RequeueCount);
            AssertMessageLease(lease, msg.LeaseExpiredAt);

            var msg2 = await queue.GetMessageAsync(lease);
            Assert.Null(msg2);

            //Pass half timeLeft of lease
            await Task.Delay((int)(lease / 2.0 * 1000));

            //The lease of msg is not expired.
            msg2 = await queue.GetMessageAsync(lease);
            Assert.Null(msg2);

            //Pass another half timeLeft of lease
            await Task.Delay((int)(lease / 2.0 * 1000));

            //The lease of msg is expired.
            await Assert.ThrowsAsync<InvalidQueueOperation>(() =>
            {
                return queue.ReturnMessageAsync(msg.Id, msg.Receipt);
            });

            //The the same message is available in queue again.
            msg2 = await queue.GetMessageAsync(lease);
            Assert.NotNull(msg2);
            Assert.Equal(msg.Id, msg2.Id);
            Assert.NotEqual(msg.Receipt, msg2.Receipt);
            Assert.Equal(1, msg2.RequeueCount);
            AssertMessageLease(lease, msg2.LeaseExpiredAt);
        });
    }

    [Fact]
    public async Task TestGetQueueStat()
    {
        await TestQueueAsync(async (queue) =>
        {
            //Initially, queue is empty.
            var stat = await queue.GetStatAsync();
            Assert.NotNull(stat);
            Assert.Equal(queue.Name, stat.Queue);
            Assert.Equal(0, stat.MessageTotal);
            Assert.Equal(0, stat.MessageAvailable);

            //Add some messages.
            var messages = new string[] { "1", "2", "3" };
            foreach (var message in messages)
            {
                await queue.PutMessageAsync(message);
            }

            //Get stat
            stat = await queue.GetStatAsync();
            Assert.NotNull(stat);
            Assert.Equal(queue.Name, stat.Queue);
            Assert.Equal(messages.Length, stat.MessageTotal);
            Assert.Equal(messages.Length, stat.MessageAvailable);

            //Get a message from queue
            var msg = await queue.GetMessageAsync();
            Assert.NotNull(msg);

            //Get stat again, number of available should -1
            stat = await queue.GetStatAsync();
            Assert.NotNull(stat);
            Assert.Equal(queue.Name, stat.Queue);
            Assert.Equal(messages.Length, stat.MessageTotal);
            Assert.Equal(messages.Length - 1, stat.MessageAvailable);
        });
    }
}
