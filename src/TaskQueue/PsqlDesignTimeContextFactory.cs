using Microsoft.EntityFrameworkCore.Design;
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

    public static void ShowCommandLineHelp()
    {
        var cmd = Environment.GetCommandLineArgs()[0];
        var name = Path.GetFileName(cmd);
        var help = $"""
Usage:
    {name} <PsqlConnectionString> [-v PsqlVersion] [-l]

Options:
    -v: Specify database server version
    -l: Log to console
""";
        Console.Error.WriteLine(help);
    }

    public static Options ProcessCommandLine(string[] args)
    {
        var options = new Options();
        try
        {
            for (var i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-h":
                        ShowCommandLineHelp();
                        Environment.Exit(0);
                        break;
                    case "-l":
                        options.LogToConsole = true;
                        break;
                    case "-v":
                        options.PgVersion = args[++i];
                        break;
                    default:
                        if (options.PgConnectionString is null)
                        {
                            options.PgConnectionString = args[i];
                        }
                        else
                        {
                            throw new ArgumentException($"Unknown argument '{args[i]}'");
                        }
                        break;
                }
            }
            Validator.ValidateObject(options, new ValidationContext(options));
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error:\n{ex}");
            ShowCommandLineHelp();
            Environment.Exit(1);
        }
        return options;
    }

    public PsqlContext CreateDbContext(string[] args)
    {
        var options = ProcessCommandLine(args);
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
