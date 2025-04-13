namespace Rz.TaskQueue.Models;

public class Message
{
    public int Id { get; set; } = default!;

    public string? Receipt { get; set; }

    public string Queue { get; set; } = default!;

    public string Content { get; set; } = default!;

    public int RequeueCount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? LeaseExpiredAt { get; set; }
}
