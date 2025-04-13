using Rz.TaskQueue;

namespace TaskQueue.Test;

public class QueueTest
{
    [Fact]
    public void TestEnqueueAndDequeue()
    {
        var queueMgr = new QueueManager();
        var queueName = "test";
        var messageLease = 60;
        var queue = queueMgr.CreateQueue(queueName, messageLease);

        try
        {
            var messages = new string[] { "1", "2", "3" };
            foreach (var message in messages)
            {
                queue.Enqueue(message);
            }

            for (var i = 0; i < messages.Length; i++)
            {
                var msg = queue.Dequeue();
                Assert.NotNull(msg);
                Assert.Equal(queueName, msg.Queue);
                Assert.Equal(messages[i], msg.Content);
                Assert.Equal(0, msg.RequeueCount);
                Assert.Equal(messageLease, (int)(msg.LeaseExpiredAt - msg.CreatedAt).TotalSeconds);
            }
            var msg2 = queue.Dequeue();
            Assert.Null(msg2);
        }
        finally
        {
            queueMgr.DeleteQueue(queueName);
        }
    }
}
