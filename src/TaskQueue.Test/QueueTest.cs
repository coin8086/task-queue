using Rz.TaskQueue;

namespace TaskQueue.Test;

public class QueueTest
{
    [Fact]
    public void TestEnqueueAndDequeue()
    {
        var queueName = "test";
        var messageLease = 60;
        var dbContextFactory = new PsqlContextFactory("connection string");
        var queue = new Queue(dbContextFactory, queueName, messageLease);
        queue.Create();
        try
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
                Assert.Equal(queueName, msg.Queue);
                Assert.Equal(messages[i], msg.Content);
                Assert.Equal(0, msg.RequeueCount);
                Assert.Equal(messageLease, (int)(msg.LeaseExpiredAt - msg.CreatedAt).TotalSeconds);
            }
            var msg2 = queue.GetMessage();
            Assert.Null(msg2);
        }
        finally
        {
            queue.Delete();
        }
    }
}
