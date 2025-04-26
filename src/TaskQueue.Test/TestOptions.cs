using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;

namespace Rz.TaskQueue.Test;

internal class TestOptions
{
    [Required]
    public string PgConnectionString { get; set; } = default!;

    public string? PgVersion { get; set; }

    public static TestOptions Instance { get; private set; }

    static TestOptions()
    {
        var builder = new ConfigurationBuilder()
            .AddJsonFile("TestSettings.json", true)
            .AddEnvironmentVariables("Test_");
        var configuration = builder.Build();

        Instance = configuration.Get<TestOptions>() ?? throw new ArgumentException("No test options are found!");

        if (string.IsNullOrEmpty(Instance.PgConnectionString))
        {
            throw new ArgumentException("Connection string must be specified!");
        }
    }
}
