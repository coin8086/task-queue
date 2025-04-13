namespace Rz.TaskQueue;

public interface IQueue
{
    //Queue name
    string Name { get; }

    //Default message lease time in seconds
    int MessageLease { get; }

    //Create the queue only if it doesn't exist.
    void Create();

    //Delete the queue only if it exists.
    void Delete();

    //Put a message at the end of the queue, with an optional lease that overrides the default lease on the queue.
    void PutMessage(string messsage, int? lease = null);

    //Get a messsage from the front of the queue.
    //Return null if no message available.
    //The returned message must be processed within the lease time. Otherwise the message will be available to
    //another call to this method. When a returned messsage has been processed, it must be deleted from the queue
    //by the DeleteMessage method. When a returned message cannot be processed, it can be deleted or returned back
    //to the queue by the ReturnMessage method.
    public IQueueMessage? GetMessage();

    //Extend the message lease time in seconds.
    void ExtendMessageLease(int messageId, string receipt, int lease);

    //Delete the message from the queue when it has been processed.
    void DeleteMessage(int messageId, string receipt);

    //Return the message back to the queue when it cannot be processed.
    void ReturnMessage(int messageId, string receipt);
}
