using System.Text.Json;

namespace Rz.TaskQueue.Client;

public class QueueStat
{
    public string Queue { get; set; } = default!;

    public int MessageTotal { get; set; }

    public int MessageAvailable { get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, ToStringOptions);
    }

    private static JsonSerializerOptions ToStringOptions { get; } = new JsonSerializerOptions() { WriteIndented = true };
}
