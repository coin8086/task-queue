namespace Rz.TaskQueue;

public interface IQueueManager
{
    //Create a queue for read and write if one with the same does not exist.
    //Queue name cannot be an empty string or "*", which are reserved for future use.
    //If messageLease is null, then a default value will be applied.
    IQueue CreateQueue(string name, int? messageLease = null);

    //Delete a queue and all its messages.
    //If force is false, then the messages that are leased (dequeued but not deleted yet)
    //will not be deleted. Otherwise, the leased messages are deleted, too.
    void DeleteQueue(string name, bool force = false);
}
