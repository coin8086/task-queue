namespace Rz.TaskQueue;

public class InvalidQueueOperation : Exception
{
}

public interface IQueue
{
    //Queue name
    string Name { get; }

    //Default message lease time in seconds
    int MessageLease { get; }

    //Create the queue only if it doesn't exist.
    Task CreateAsync();

    //Delete the queue only if it exists.
    Task DeleteAsync();

    Task<IQueueStat> GetStatAsync();

    //Put a message at the end of the queue.
    Task PutMessageAsync(string messsage);

    //Get a messsage from the front of the queue, with an optional lease that overrides the default lease on the queue.
    //Return null if no message available.
    //The returned message must be processed within the lease time. Otherwise the message will be available to
    //another call to this method. When a returned messsage has been processed, it must be deleted from the queue
    //by the DeleteMessage method. When a returned message cannot be processed, it can be deleted or returned back
    //to the queue by the ReturnMessage method.
    Task<IQueueMessage?> GetMessageAsync(int? lease = null);

    //Extend the message lease time in seconds.
    //When lease is null, the default lease on queue will be used.
    //Throw InvalidQueueOperation when lease is expired or messageId is invalid.
    Task ExtendMessageLeaseAsync(int messageId, string receipt, int? lease = null);

    //Delete the message from the queue when it has been processed.
    //Throw InvalidQueueOperation when lease is expired or messageId is invalid.
    Task DeleteMessageAsync(int messageId, string receipt);

    //Return the message back to the queue when it cannot be processed.
    //Throw InvalidQueueOperation when lease is expired or messageId is invalid.
    Task ReturnMessageAsync(int messageId, string receipt);
}
