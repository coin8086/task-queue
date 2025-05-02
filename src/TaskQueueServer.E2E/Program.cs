using System.ComponentModel.DataAnnotations;
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
            Console.WriteLine($"Delete queue {queue}.");
            var response = await client.DeleteAsync($"queues/{queue}");
            response.EnsureSuccessStatusCode();
        }

        {
            Console.WriteLine($"Create queue {queue}.");
            var response = await client.PostAsJsonAsync("queues", queue);
            response.EnsureSuccessStatusCode();
        }

        {
            Console.WriteLine("Put messages in queue.");
            var messages = new string[] { "m1", "m2", "m3" };
            foreach (var message in messages)
            {
                var response = await client.PostAsJsonAsync($"queues/{queue}/in", message);
                response.EnsureSuccessStatusCode();
            }
        }

        await GetMessagesFromQueue(client, queue);

        {
            Console.WriteLine("Wait for lease expiration.");
            await Task.Delay(3 * 1000);
        }

        await GetMessagesFromQueue(client, queue, async (msg) =>
        {
            try
            {
                //Process the msg
                //...

                //Extend the message lease when in need
                var response = await client.PostAsJsonAsync($"queues/{queue}/messages/{msg.Id},{msg.Receipt}/lease", 2);
                response.EnsureSuccessStatusCode();

                //Delete message from queue in the end
                response = await client.DeleteAsync($"queues/{queue}/messages/{msg.Id},{msg.Receipt}");
                response.EnsureSuccessStatusCode();
            }
            catch // Catch some application exception
            {
                //Return message to queue if it cannot be handled.
                await client.PostAsync($"queues/{queue}/messages/{msg.Id},{msg.Receipt}/return", null);
            }
        });

        //There should be no message in queue now.
        await GetMessagesFromQueue(client, queue);
    }

    static async Task GetMessagesFromQueue(HttpClient client, string queue, Func<QueueMessage, Task>? messageHandler = null)
    {
        Console.WriteLine("Get messages from queue.");
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
}
