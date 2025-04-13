namespace Rz.TaskQueue;

public class QueueManager : IQueueManager
{
    public IQueue CreateQueue(string name, int? messageLease = null)
    {
        throw new NotImplementedException();
    }

    public void DeleteQueue(string name, bool force = false)
    {
        throw new NotImplementedException();
    }
}
