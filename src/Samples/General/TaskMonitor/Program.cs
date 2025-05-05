namespace TaskMonitor;

public class Program
{
    public static void Main(string[] args)
    {
        var switchMappings = new Dictionary<string, string>()
        {
            { "-L", "Logging:LogLevel:TaskMonitor" },
            { "-e", "Queue:EndPoint" },
            { "-q", "Monitor:Queue" },
            { "-c", "ProcessWorkerProvider:Command" },
            { "-a", "ProcessWorkerProvider:Arguments" },
        };

        var builder = WebApplication.CreateBuilder(args);
        builder.Configuration.AddCommandLine(args, switchMappings);
        builder.Services.AddControllers();
        builder.Services.AddMonitor();
        builder.Services.AddQueueClient();
        builder.Services.AddFixedScalePolicy();
        builder.Services.AddProcessWorkerProvider();

        var app = builder.Build();
        app.MapControllers();
        app.Run();
    }
}
