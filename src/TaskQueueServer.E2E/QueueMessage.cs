using System.Text.Json;

namespace TaskQueueServer.E2E;

internal class QueueMessage
{
    public int Id { get; set; }

    public string Receipt { get; set; } = default!;

    public string Queue { get; set; } = default!;

    public string Content { get; set; } = default!;

    public int RequeueCount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset LeaseExpiredAt { get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions() { WriteIndented = true });
    }
}
