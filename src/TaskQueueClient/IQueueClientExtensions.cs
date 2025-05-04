namespace Rz.TaskQueue.Client;

public static class IQueueClientExtensions
{
    public static async Task<QueueMessage> WaitMessageAsync(
        this IQueueClient client,
        string queue,
        int? lease /*in second*/ = null,
        int queryInterval /*in millisecond*/ = 2000,
        CancellationToken token = default)
    {
        while (true)
        {
            token.ThrowIfCancellationRequested();
            var message = await client.GetMessageAsync(queue, lease, token).ConfigureAwait(false);
            if (message != null)
            {
                return message;
            }
            await Task.Delay(queryInterval, token).ConfigureAwait(false);
        }
    }
}
