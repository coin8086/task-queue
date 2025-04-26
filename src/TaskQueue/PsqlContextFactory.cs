using Microsoft.EntityFrameworkCore;

namespace Rz.TaskQueue;

public class PsqlContextFactory : IDbContextFactory<PsqlContext>
{
    private readonly string _dbConnectionString;

    private readonly Version? _dbVersion;

    private readonly Action<DbContextOptionsBuilder<PsqlContext>>? _configure;

    public PsqlContextFactory(string pgConnectionString, string? pgVersion = null,
        Action<DbContextOptionsBuilder<PsqlContext>>? configure = null)
    {
        _dbConnectionString = pgConnectionString;
        if (pgVersion != null)
        {
            _dbVersion = new Version(pgVersion);
        }
        _configure = configure;
    }

    public PsqlContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<PsqlContext>()
            .UseNpgsql(_dbConnectionString, options => options.SetPostgresVersion(_dbVersion));
        _configure?.Invoke(optionsBuilder);
        return new PsqlContext(optionsBuilder.Options);
    }
}
