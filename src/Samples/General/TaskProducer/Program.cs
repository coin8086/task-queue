using Microsoft.Extensions.Configuration;
using Rz.TaskQueue.Client;
using System.ComponentModel.DataAnnotations;

namespace TaskProducer;

class Program
{
    class Options : IValidatableObject
    {
        [Required]
        [Url]
        public string EndPoint { get; set; } = default!;

        [Required]
        public string Queue { get; set; } = default!;

        public int From { get; set; }

        public int To { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (From > To)
            {
                yield return new ValidationResult("From must not be greater than To.", [nameof(From), nameof(To)]);
            }
        }
    }

    static Options GetOptions(string[] args)
    {
        var switchMappings = new Dictionary<string, string>()
        {
            { "-e", "EndPoint" },
            { "-q", "Queue" },
            { "-f", "From" },
            { "-t", "To" },
        };

        var builder = new ConfigurationBuilder()
            .AddEnvironmentVariables("P_")
            .AddCommandLine(args, switchMappings);

        var configuration = builder.Build();
        var options = configuration.Get<Options>();

        ArgumentNullException.ThrowIfNull(options, nameof(options));
        Validator.ValidateObject(options, new ValidationContext(options));

        return options;
    }

    static void ShowHelp(string? error = null)
    {
        var help = $"""
{nameof(TaskProducer)} <-e EndPoint> <-q Queue> [-f From] [-t To]

where EndPoint is in the form "http://host:port" or "https://host:port".
""";

        Console.WriteLine(help);
        if (error != null)
        {
            Console.WriteLine(error);
        }
    }

    static int Main(string[] args)
    {
        Options options;
        try
        {
            options = GetOptions(args);
        }
        catch (Exception ex)
        {
            ShowHelp(ex.ToString());
            return -1;
        }

        var client = new QueueClient(options.EndPoint);

        Console.WriteLine($"Create queue {options.Queue}.");
        client.CreateQueueAsync(options.Queue).Wait();

        var tasks = new Task[options.To -  options.From + 1];

        Console.WriteLine($"Send {tasks.Length} task(s).");
        for (var i = 0; i < tasks.Length; i++)
        {
            tasks[i] = client.PutMessageAsync(options.Queue, (options.From + i).ToString());
        }

        Console.WriteLine($"Wait for {tasks.Length} task(s).");
        Task.WaitAll(tasks);

        Console.WriteLine("Done.");
        return 0;
    }
}
