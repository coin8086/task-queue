
namespace Rz.TaskQueue;

//NOTE: This class is very similar to Rz.TaskQueue.Models.Messages, only that
//the nullability of some properties is different.
internal class QueueMessage : IQueueMessage
{
    public int Id {  get; set; }

    public string Receipt { get; set; } = default!;

    public string Queue { get; set; } = default!;

    public string Content { get; set; } = default!;

    public int RequeueCount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset LeaseExpiredAt { get; set; }
}
