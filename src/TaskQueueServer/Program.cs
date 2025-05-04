namespace Rz.TaskQueue.Server;

public class Program
{
    public static void Main(string[] args)
    {
        var switchMappings = new Dictionary<string, string>()
        {
            { "-L", "Logging:LogLevel:Default" },
        };

        var builder = WebApplication.CreateBuilder(args);
        builder.Configuration.AddCommandLine(args, switchMappings);
        builder.Services.AddControllers();
        builder.Services.AddPsqlContextFactory();

        var app = builder.Build();
        app.UseMiddleware<ErrorHandler>();
        app.MapControllers();
        app.Run();
    }
}
