using System.Text.Json;

namespace TaskQueueServer.E2E;

internal class QueueStat
{
    public string Queue { get; set; } = default!;

    public int MessageTotal {  get; set; }

    public int MessageAvailable {  get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions() { WriteIndented = true });
    }
}
