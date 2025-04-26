using Microsoft.EntityFrameworkCore;

namespace Rz.TaskQueue.Models;

[Index(nameof(Queue))]
[Index(nameof(CreatedAt))]
[Index(nameof(LeaseExpiredAt))]
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
