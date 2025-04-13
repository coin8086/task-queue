using Microsoft.EntityFrameworkCore;

namespace Rz.TaskQueue;

public class PsqlContextFactory : IDbContextFactory<PsqlContext>
{
    public PsqlContextFactory(string pgConnectionString, string? pgVersion = null)
    {
        throw new NotImplementedException();
    }

    public PsqlContext CreateDbContext()
    {
        throw new NotImplementedException();
    }
}
