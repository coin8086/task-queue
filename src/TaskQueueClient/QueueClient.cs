using System.Net.Http.Json;

namespace Rz.TaskQueue.Client;

public class QueueClient : IQueueClient
{
    private readonly HttpClient _client;

    public QueueClient(string endPoint)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(endPoint, nameof(endPoint));
        if (!endPoint.EndsWith("/"))
        {
            endPoint += "/";
        }
        endPoint += "api/v1/";
        var url = new Uri(endPoint);
        _client = new HttpClient();
        _client.BaseAddress = url;
    }

    public async Task CreateQueueAsync(string queue)
    {
        var response = await _client.PostAsJsonAsync("queues", queue);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteQueueAsync(string queue)
    {
        var response = await _client.DeleteAsync($"queues/{queue}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<QueueStat> GetQueueStatAsync(string queue)
    {
        var stat = await _client.GetFromJsonAsync<QueueStat>($"queues/{queue}/stat") ?? throw new ApplicationException();
        return stat;
    }

    public async Task PutMessageAsync(string queue, string message)
    {
        var response = await _client.PostAsJsonAsync($"queues/{queue}/in", message);
        response.EnsureSuccessStatusCode();
    }

    public async Task<QueueMessage?> GetMessageAsync(string queue, int? lease = null)
    {
        HttpResponseMessage? response = null;
        if (lease != null)
        {
            response = await _client.PostAsJsonAsync($"queues/{queue}/out", 2);
        }
        else
        {
            response = await _client.PostAsync($"queues/{queue}/out", null);
        }
        response.EnsureSuccessStatusCode();

        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<QueueMessage>() ?? throw new ApplicationException();
    }

    public async Task ExtendMessageLeaseAsync(string queue, int messageId, string receipt, int? lease = null)
    {
        HttpResponseMessage? response = null;
        if (lease != null)
        {
            response = await _client.PostAsJsonAsync($"queues/{queue}/messages/{messageId}/lease?receipt={receipt}", 2);
        }
        else
        {
            response = await _client.PostAsync($"queues/{queue}/messages/{messageId}/lease?receipt={receipt}", null);
        }
        response.EnsureSuccessStatusCode();
    }

    public async Task ReturnMessageAsync(string queue, int messageId, string receipt)
    {
        var response = await _client.PostAsync($"queues/{queue}/messages/{messageId}/return?receipt={receipt}", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteMessageAsync(string queue, int messageId, string receipt)
    {
        var response = await _client.DeleteAsync($"queues/{queue}/messages/{messageId}?receipt={receipt}");
        response.EnsureSuccessStatusCode();
    }
}
