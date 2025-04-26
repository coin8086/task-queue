using Microsoft.EntityFrameworkCore;

namespace Rz.TaskQueue.Test;

public class TestDatabaseFixture
{
    public TestDatabaseFixture()
    {
        RecreateDb();
    }

    public static void RecreateDb()
    {
        using var context = CreateContext();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }

    public static PsqlContext CreateContext()
    {
        return CreateContextFactory().CreateDbContext();
    }

    public static PsqlContextFactory CreateContextFactory(Action<DbContextOptionsBuilder<PsqlContext>>? configure = null)
    {
        return new PsqlContextFactory(TestOptions.Instance.PgConnectionString, TestOptions.Instance.PgVersion, configure);
    }
}

[CollectionDefinition("DbTest")]
public class CollectionDefinition : ICollectionFixture<TestDatabaseFixture>
{
}
