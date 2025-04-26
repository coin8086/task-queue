using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace Rz.TaskQueue.Server;

public class PsqlContextFactoryOptions
{
    [Required]
    public string PgConnectionString { get; set; } = default!;

    public string? PgVersion { get; set; }
}

public static class PsqlContextFactoryIServiceCollectionExtensions
{
    public static IServiceCollection AddPsqlContextFactory(this IServiceCollection services)
    {
        services.AddOptionsWithValidateOnStart<PsqlContextFactoryOptions>()
            .BindConfiguration("PostgreSQL")
            .ValidateDataAnnotations();

        return services.AddSingleton<IDbContextFactory<PsqlContext>>(services => {
            var options = services.GetRequiredService<IOptions<PsqlContextFactoryOptions>>();
            return new PsqlContextFactory(options.Value.PgConnectionString, options.Value.PgVersion);
        });
    }
}
