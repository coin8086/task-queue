
namespace Rz.TaskQueue.Server;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddPsqlContextFactory();

        var app = builder.Build();
        app.UseMiddleware<ErrorHandler>();
        app.MapControllers();
        app.Run();
    }
}
