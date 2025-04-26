
using Microsoft.EntityFrameworkCore;

namespace Rz.TaskQueue.Server;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddPsqlContextFactory();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<PsqlContext>>();
            using var dbContext = dbContextFactory.CreateDbContext();
            //dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
        }

        app.MapControllers();
        app.Run();
    }
}
