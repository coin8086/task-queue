using Microsoft.Extensions.Options;
using Rz.TaskQueue.Client;
using System.ComponentModel.DataAnnotations;

namespace TaskMonitor;

public class QueueOptions
{
    [Required]
    [Url]
    public string EndPoint { get; set; } = default!;
}

public static partial class IServiceCollectionExtensions
{
    public static IServiceCollection AddQueueClient(this IServiceCollection services)
    {
        services.AddOptionsWithValidateOnStart<QueueOptions>()
            .BindConfiguration("Queue")
            .ValidateDataAnnotations();

        services.AddSingleton<IQueueClient>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<QueueOptions>>();
            return new QueueClient(options.Value.EndPoint);
        });

        return services;
    }
}
