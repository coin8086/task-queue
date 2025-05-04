namespace TaskConsumer;

public class Program
{
    public static void Main(string[] args)
    {
        var switchMappings = new Dictionary<string, string>()
        {
            { "-L", "Logging:LogLevel:Default" },
            { "-e", "Queue:EndPoint" },
            { "-q", "Worker:Queue" },
        };

        var builder = Host.CreateApplicationBuilder(args);
        builder.Configuration.AddCommandLine(args, switchMappings);
        builder.Services.AddWorkerService();
        builder.Services.AddQueueClient();

        var host = builder.Build();
        host.Run();
    }
}
