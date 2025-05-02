using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;

namespace Rz.TaskQueue;

public class PsqlDesignTimeContextFactory : IDesignTimeDbContextFactory<PsqlContext>
{
    public class Options
    {
        [Required]
        public string PgConnectionString { get; set; } = default!;

        public string? PgVersion { get; set; }

        public bool LogToConsole { get; set; }
    }

    public PsqlContext CreateDbContext(string[] args)
    {
        var switchMappings = new Dictionary<string, string>()
        {
            { "-c", "PgConnectionString" },
            { "-v", "PgVersion" },
            { "-l", "LogToConsole" },
        };

        var builder = new ConfigurationBuilder()
            .AddEnvironmentVariables("RZ_")
            .AddCommandLine(args, switchMappings);

        var configuration = builder.Build();
        var options = configuration.Get<Options>();

        if (options == null)
        {
            throw new ArgumentException("PgConnectionString must be specified!");
        }
        Validator.ValidateObject(options, new ValidationContext(options));

        var factory = new PsqlContextFactory(options.PgConnectionString, options.PgVersion, (optBuilder) =>
        {
            if (options.LogToConsole)
            {
                optBuilder.LogTo(Console.Error.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
            }
        });
        return factory.CreateDbContext();
    }
}
