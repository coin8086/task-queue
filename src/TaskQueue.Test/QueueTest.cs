namespace Rz.TaskQueue.Test;

public class QueueTest
{
    private const int DefaultMessageLease = 60;

    private const string Letters = "abcdefghijklmnopqrstuvwxyz0123456789";

    private static string RandomQueueName => $"queue_{new string(Random.Shared.GetItems<char>(Letters, 6))}";

    private static async Task TestQueueAsync(Func<IQueue, Task> test)
    {
        var dbContextFactory = new PsqlContextFactory("connection string");
        IQueue queue = new Queue(dbContextFactory, RandomQueueName, DefaultMessageLease);
        queue.Create();
        try
        {
            await test(queue).ConfigureAwait(false);
        }
        finally
        {
            queue.Delete();
        }
    }

    private static void AssertMessageLease(int lease, DateTimeOffset leaseExpiredAt, DateTimeOffset? now = null)
    {
        var timeLeft = leaseExpiredAt - (now ?? DateTimeOffset.Now);
        Assert.True(timeLeft.TotalSeconds > 0 && timeLeft.TotalSeconds <= lease);
    }

    [Fact]
    public void TestCreateAndDeleteQueue()
    {
        var dbContextFactory = new PsqlContextFactory("connection string");
        IQueue queue = new Queue(dbContextFactory, RandomQueueName, DefaultMessageLease);

        queue.Create();
        try
        {
            var messages = new string[] { "1", "2", "3" };
            foreach (var message in messages)
            {
                queue.PutMessage(message);
            }

            var msg = queue.GetMessage();
            Assert.NotNull(msg);

            queue.Delete();

            //After queue is deleted, no message can be retrieved from it.
            var msg2 = queue.GetMessage();
            Assert.Null(msg2);

            //And any operations on the retrieved msg should fail.
            Assert.Throws<Exception>(() =>
            {
                queue.ExtendMessageLease(msg.Id, msg.Receipt, queue.MessageLease);
            });

            Assert.Throws<Exception>(() =>
            {
                queue.ReturnMessage(msg.Id, msg.Receipt);
            });

            Assert.Throws<Exception>(() =>
            {
                queue.DeleteMessage(msg.Id, msg.Receipt);
            });
        }
        finally
        {
            queue.Delete();
        }
    }

    [Fact]
    public async Task TestEnqueueAndDequeue()
    {
        await TestQueueAsync(queue =>
        {
            var messages = new string[] { "1", "2", "3" };
            foreach (var message in messages)
            {
                queue.PutMessage(message);
            }

            for (var i = 0; i < messages.Length; i++)
            {
                var msg = queue.GetMessage();
                Assert.NotNull(msg);
                Assert.Equal(queue.Name, msg.Queue);
                Assert.Equal(messages[i], msg.Content);
                Assert.Equal(0, msg.RequeueCount);
                AssertMessageLease(queue.MessageLease, msg.LeaseExpiredAt);
            }

            //When no visible/available message in queue, null is returned.
            var msg2 = queue.GetMessage();
            Assert.Null(msg2);

            return Task.CompletedTask;
        });
    }

    [Fact]
    public async Task TestMessageLeaseTimeout()
    {
        await TestQueueAsync(async queue =>
        {
            var msgContent = "abc";
            var lease = 2;
            queue.PutMessage(msgContent);

            var msg = queue.GetMessage(lease);
            Assert.NotNull(msg);
            Assert.Equal(msgContent, msg.Content);
            AssertMessageLease(lease, msg.LeaseExpiredAt);

            var msg2 = queue.GetMessage(lease);
            Assert.Null(msg2);

            //Pass half timeLeft of lease
            await Task.Delay((int)(lease / 2.0 * 1000));

            //The lease of msg is not expired.
            msg2 = queue.GetMessage(lease);
            Assert.Null(msg2);

            //Pass another half timeLeft of lease
            await Task.Delay((int)(lease / 2.0 * 1000));

            //The lease of msg is expired.
            //The the same message is available in queue again.
            msg2 = queue.GetMessage(lease);
            Assert.NotNull(msg2);
            Assert.Equal(msg.Id, msg2.Id);
            Assert.NotEqual(msg.Receipt, msg2.Receipt);
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
            queue.PutMessage(msgContent);

            var msg = queue.GetMessage(lease);
            Assert.NotNull(msg);
            Assert.Equal(msgContent, msg.Content);
            AssertMessageLease(lease, msg.LeaseExpiredAt);

            var msg2 = queue.GetMessage(lease);
            Assert.Null(msg2);

            //Pass half timeLeft of lease
            await Task.Delay((int)(lease / 2.0 * 1000));

            //The lease of msg is not expired.
            msg2 = queue.GetMessage(lease);
            Assert.Null(msg2);

            queue.ExtendMessageLease(msg.Id, msg.Receipt, lease);

            //Pass another half timeLeft of lease
            await Task.Delay((int)(lease / 2.0 * 1000));

            //The extended lease of msg is not expired.
            msg2 = queue.GetMessage(lease);
            Assert.Null(msg2);

            //Pass a third half timeLeft of lease
            await Task.Delay((int)(lease / 2.0 * 1000));

            //The extended lease of msg is expired.
            msg2 = queue.GetMessage(lease);
            Assert.NotNull(msg2);
            Assert.Equal(msg.Id, msg2.Id);
            Assert.NotEqual(msg.Receipt, msg2.Receipt);
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
            queue.PutMessage(msgContent);

            var msg = queue.GetMessage(lease);
            Assert.NotNull(msg);
            Assert.Equal(msgContent, msg.Content);
            AssertMessageLease(lease, msg.LeaseExpiredAt);

            var msg2 = queue.GetMessage(lease);
            Assert.Null(msg2);

            //Pass half timeLeft of lease
            await Task.Delay((int)(lease / 2.0 * 1000));

            //The lease of msg is not expired.
            msg2 = queue.GetMessage(lease);
            Assert.Null(msg2);

            //Pass another half timeLeft of lease
            await Task.Delay((int)(lease / 2.0 * 1000));

            //The lease of msg is expired.
            Assert.Throws<Exception>(() =>
            {
                queue.ExtendMessageLease(msg.Id, msg.Receipt, lease);
            });

            //The the same message is available in queue again.
            msg2 = queue.GetMessage(lease);
            Assert.NotNull(msg2);
            Assert.Equal(msg.Id, msg2.Id);
            Assert.NotEqual(msg.Receipt, msg2.Receipt);
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
            queue.PutMessage(msgContent);

            var msg = queue.GetMessage(lease);
            Assert.NotNull(msg);
            Assert.Equal(msgContent, msg.Content);
            AssertMessageLease(lease, msg.LeaseExpiredAt);

            var msg2 = queue.GetMessage(lease);
            Assert.Null(msg2);

            //Pass half timeLeft of lease
            await Task.Delay((int)(lease / 2.0 * 1000));

            //The lease of msg is not expired.
            msg2 = queue.GetMessage(lease);
            Assert.Null(msg2);

            queue.DeleteMessage(msg.Id, msg.Receipt);

            //Pass another half timeLeft of lease
            await Task.Delay((int)(lease / 2.0 * 1000));

            //No more message in the queue.
            msg2 = queue.GetMessage(lease);
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
            queue.PutMessage(msgContent);

            var msg = queue.GetMessage(lease);
            Assert.NotNull(msg);
            Assert.Equal(msgContent, msg.Content);
            AssertMessageLease(lease, msg.LeaseExpiredAt);

            var msg2 = queue.GetMessage(lease);
            Assert.Null(msg2);

            //Pass half timeLeft of lease
            await Task.Delay((int)(lease / 2.0 * 1000));

            //The lease of msg is not expired.
            msg2 = queue.GetMessage(lease);
            Assert.Null(msg2);

            //Pass another half timeLeft of lease
            await Task.Delay((int)(lease / 2.0 * 1000));

            //The lease of msg is expired.
            Assert.Throws<Exception>(() =>
            {
                queue.DeleteMessage(msg.Id, msg.Receipt);
            });

            //The the same message is available in queue again.
            msg2 = queue.GetMessage(lease);
            Assert.NotNull(msg2);
            Assert.Equal(msg.Id, msg2.Id);
            Assert.NotEqual(msg.Receipt, msg2.Receipt);
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
            queue.PutMessage(msgContent);

            var msg = queue.GetMessage();
            Assert.NotNull(msg);
            Assert.Equal(msgContent, msg.Content);
            AssertMessageLease(lease, msg.LeaseExpiredAt);

            var msg2 = queue.GetMessage(lease);
            Assert.Null(msg2);

            //Pass half timeLeft of lease
            await Task.Delay((int)(lease / 2.0 * 1000));

            //The lease of msg is not expired.
            msg2 = queue.GetMessage(lease);
            Assert.Null(msg2);

            queue.ReturnMessage(msg.Id, msg.Receipt);

            //The the same message is available in queue again.
            msg2 = queue.GetMessage(lease);
            Assert.NotNull(msg2);
            Assert.Equal(msg.Id, msg2.Id);
            Assert.NotEqual(msg.Receipt, msg2.Receipt);
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
            queue.PutMessage(msgContent);

            var msg = queue.GetMessage(lease);
            Assert.NotNull(msg);
            Assert.Equal(msgContent, msg.Content);
            AssertMessageLease(lease, msg.LeaseExpiredAt);

            var msg2 = queue.GetMessage(lease);
            Assert.Null(msg2);

            //Pass half timeLeft of lease
            await Task.Delay((int)(lease / 2.0 * 1000));

            //The lease of msg is not expired.
            msg2 = queue.GetMessage(lease);
            Assert.Null(msg2);

            //Pass another half timeLeft of lease
            await Task.Delay((int)(lease / 2.0 * 1000));

            //The lease of msg is expired.
            Assert.Throws<Exception>(() =>
            {
                queue.ReturnMessage(msg.Id, msg.Receipt);
            });

            //The the same message is available in queue again.
            msg2 = queue.GetMessage(lease);
            Assert.NotNull(msg2);
            Assert.Equal(msg.Id, msg2.Id);
            Assert.NotEqual(msg.Receipt, msg2.Receipt);
            AssertMessageLease(lease, msg2.LeaseExpiredAt);
        });
    }
}
