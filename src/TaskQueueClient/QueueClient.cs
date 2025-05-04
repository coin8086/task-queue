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

    public async Task CreateQueueAsync(string queue, CancellationToken token = default)
    {
        var response = await _client.PostAsJsonAsync("queues", queue, token);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteQueueAsync(string queue, CancellationToken token = default)
    {
        var response = await _client.DeleteAsync($"queues/{queue}", token);
        response.EnsureSuccessStatusCode();
    }

    public async Task<QueueStat> GetQueueStatAsync(string queue, CancellationToken token = default)
    {
        var stat = await _client.GetFromJsonAsync<QueueStat>($"queues/{queue}/stat", token) ?? throw new ApplicationException();
        return stat;
    }

    public async Task PutMessageAsync(string queue, string message, CancellationToken token = default)
    {
        var response = await _client.PostAsJsonAsync($"queues/{queue}/in", message, token);
        response.EnsureSuccessStatusCode();
    }

    public async Task<QueueMessage?> GetMessageAsync(string queue, int? lease = null, CancellationToken token = default)
    {
        HttpResponseMessage? response = null;
        if (lease != null)
        {
            response = await _client.PostAsJsonAsync($"queues/{queue}/out", 2, token);
        }
        else
        {
            response = await _client.PostAsync($"queues/{queue}/out", null, token);
        }
        response.EnsureSuccessStatusCode();

        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<QueueMessage>() ?? throw new ApplicationException();
    }

    public async Task ExtendMessageLeaseAsync(string queue, int messageId, string receipt, int? lease = null, CancellationToken token = default)
    {
        HttpResponseMessage? response = null;
        if (lease != null)
        {
            response = await _client.PostAsJsonAsync($"queues/{queue}/messages/{messageId}/lease?receipt={receipt}", 2, token);
        }
        else
        {
            response = await _client.PostAsync($"queues/{queue}/messages/{messageId}/lease?receipt={receipt}", null, token);
        }
        response.EnsureSuccessStatusCode();
    }

    public async Task ReturnMessageAsync(string queue, int messageId, string receipt, CancellationToken token = default)
    {
        var response = await _client.PostAsync($"queues/{queue}/messages/{messageId}/return?receipt={receipt}", null, token);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteMessageAsync(string queue, int messageId, string receipt, CancellationToken token = default)
    {
        var response = await _client.DeleteAsync($"queues/{queue}/messages/{messageId}?receipt={receipt}", token);
        response.EnsureSuccessStatusCode();
    }
}
