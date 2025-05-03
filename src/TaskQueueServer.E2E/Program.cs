using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net.Http.Json;

namespace TaskQueueServer.E2E;

class Program
{
    class Options
    {
        [Required]
        [Url]
        public string EndPoint { get; set; } = default!;
    }

    static Options ParseCommandLine(string[] args)
    {
        var options = new Options();
        try
        {
            options.EndPoint = args[0];
            Validator.ValidateObject(options, new ValidationContext(options));
        }
        catch (Exception)
        {
            ShowHelp();
            throw new ArgumentException();
        }

        if (!options.EndPoint.EndsWith("/"))
        {
            options.EndPoint += "/";
        }

        return options;
    }

    static void ShowHelp()
    {
        var help = $"""
{nameof(E2E)} <EndPoint>

where EndPoint is in the form "http://host:port" or "https://host:port".
""";
        Console.WriteLine(help);
    }

    static async Task Main(string[] args)
    {
        var options = ParseCommandLine(args);
        var baseAddress = options.EndPoint + "api/v1/";
        Console.WriteLine($"BaseAddress: {baseAddress}");

        var client = new HttpClient();
        client.BaseAddress = new Uri(baseAddress);

        var queue = "q1";

        {
            Console.WriteLine($"[{DateTimeOffset.UtcNow:o}] Delete queue {queue}.");
            var response = await client.DeleteAsync($"queues/{queue}");
            response.EnsureSuccessStatusCode();
        }

        {
            Console.WriteLine($"[{DateTimeOffset.UtcNow:o}] Create queue {queue}.");
            var response = await client.PostAsJsonAsync("queues", queue);
            response.EnsureSuccessStatusCode();
        }

        {
            Console.WriteLine($"[{DateTimeOffset.UtcNow:o}] Put messages in queue.");
            var messages = new string[] { "m1", "m2", "m3" };
            foreach (var message in messages)
            {
                var response = await client.PostAsJsonAsync($"queues/{queue}/in", message);
                response.EnsureSuccessStatusCode();
            }
        }

        await GetQueueStat(client, queue);

        QueueMessage? qmsg = null;
        await GetMessagesFromQueue(client, queue, (msg) =>
        {
            qmsg = msg;
            return Task.CompletedTask;
        });
        Trace.Assert(qmsg != null);

        await GetQueueStat(client, queue);

        {
            Console.WriteLine($"[{DateTimeOffset.UtcNow:o}] Wait for lease expiration.");
            await Task.Delay(3 * 1000);
        }

        await GetQueueStat(client, queue);

        {
            Console.WriteLine($"[{DateTimeOffset.UtcNow:o}] Operations on a message that has an expired lease should fail.");
            var response = await client.PostAsJsonAsync($"queues/{queue}/messages/{qmsg.Id}/lease?receipt={qmsg.Receipt}", 2);
            Console.WriteLine($"Status code: {response.StatusCode}");
            Trace.Assert(response.StatusCode == System.Net.HttpStatusCode.NotFound);
        }

        //Now the messages are available in queue again.
        await GetMessagesFromQueue(client, queue, async (msg) =>
        {
            try
            {
                //Process the msg
                //...

                //Extend the message lease when in need
                var response = await client.PostAsJsonAsync($"queues/{queue}/messages/{msg.Id}/lease?receipt={msg.Receipt}", 2);
                response.EnsureSuccessStatusCode();

                //Delete message from queue in the end
                response = await client.DeleteAsync($"queues/{queue}/messages/{msg.Id}?receipt={msg.Receipt}");
                response.EnsureSuccessStatusCode();
            }
            catch // Catch some application exception
            {
                //Return message to queue if it cannot be handled.
                await client.PostAsync($"queues/{queue}/messages/{msg.Id}/return?receipt={msg.Receipt}", null);
            }
        });

        //There should be no message in queue now.
        await GetMessagesFromQueue(client, queue);

        await GetQueueStat(client, queue);
    }

    static async Task GetMessagesFromQueue(HttpClient client, string queue, Func<QueueMessage, Task>? messageHandler = null)
    {
        Console.WriteLine($"[{DateTimeOffset.UtcNow:o}] Get messages from queue.");
        while (true)
        {
            var response = await client.PostAsJsonAsync($"queues/{queue}/out", 2);
            response.EnsureSuccessStatusCode();

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                Console.WriteLine("No more messages in queue.");
                break;
            }

            var message = await response.Content.ReadFromJsonAsync<QueueMessage>();
            Console.WriteLine($"Message:\n{message}");

            if (messageHandler != null)
            {
                await messageHandler(message!);
            }
        }
    }

    static async Task GetQueueStat(HttpClient client, string queue)
    {
        Console.WriteLine($"[{DateTimeOffset.UtcNow:o}] Get queue stat.");
        var stat = await client.GetFromJsonAsync<QueueStat>($"queues/{queue}/stat");
        Trace.Assert(stat != null);
        Console.WriteLine(stat);
    }
}
