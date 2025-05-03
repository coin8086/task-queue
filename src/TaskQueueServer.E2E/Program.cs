using Rz.TaskQueue.Client;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Rz.TaskQueue.Server.E2E;

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
        var client = new QueueClient(options.EndPoint);
        var queue = "q1";

        {
            Console.WriteLine($"[{DateTimeOffset.UtcNow:o}] Delete queue {queue}.");
            await client.DeleteQueueAsync(queue);
        }

        {
            Console.WriteLine($"[{DateTimeOffset.UtcNow:o}] Create queue {queue}.");
            await client.CreateQueueAsync(queue);
        }

        {
            Console.WriteLine($"[{DateTimeOffset.UtcNow:o}] Put messages in queue.");
            var messages = new string[] { "m1", "m2", "m3" };
            foreach (var message in messages)
            {
                await client.PutMessageAsync(queue, message);
            }
        }

        await GetQueueStat(client, queue);

        QueueMessage? qmsg = null;
        {
            await GetMessagesFromQueue(client, queue, (msg) =>
            {
                qmsg = msg;
                return Task.CompletedTask;
            });
            Trace.Assert(qmsg != null);
        }

        await GetQueueStat(client, queue);

        {
            Console.WriteLine($"[{DateTimeOffset.UtcNow:o}] Wait for lease expiration.");
            await Task.Delay(3 * 1000);
        }

        await GetQueueStat(client, queue);

        {
            Console.WriteLine($"[{DateTimeOffset.UtcNow:o}] Operations on a message that has an expired lease should fail.");

            try
            {
                await client.ExtendMessageLeaseAsync(queue, qmsg.Id, qmsg.Receipt, 2);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error: {ex}");
            }
        }

        //Now the messages are available in queue again.
        await GetMessagesFromQueue(client, queue, async (msg) =>
        {
            try
            {
                //Process the msg
                //...

                //Extend the message lease when in need
                await client.ExtendMessageLeaseAsync(queue, msg.Id, msg.Receipt, 2);

                //Delete message from queue in the end
                await client.DeleteMessageAsync(queue, msg.Id, msg.Receipt);
            }
            catch (OperationCanceledException) // Catch some application exception
            {
                //Return message to queue if it cannot be handled.
                await client.ReturnMessageAsync(queue, msg.Id, msg.Receipt);
            }
        });

        await GetQueueStat(client, queue);
    }

    static async Task GetMessagesFromQueue(QueueClient client, string queue, Func<QueueMessage, Task>? messageHandler = null)
    {
        Console.WriteLine($"[{DateTimeOffset.UtcNow:o}] Get messages from queue.");
        while (true)
        {
            var message = await client.GetMessageAsync(queue, 2);
            if (message == null)
            {
                Console.WriteLine("No more messages in queue.");
                break;
            }

            Console.WriteLine($"Message:\n{message}");

            if (messageHandler != null)
            {
                await messageHandler(message!);
            }
        }
    }

    static async Task GetQueueStat(QueueClient client, string queue)
    {
        Console.WriteLine($"[{DateTimeOffset.UtcNow:o}] Get queue stat.");
        var stat = await client.GetQueueStatAsync(queue);
        Console.WriteLine(stat);
    }
}
