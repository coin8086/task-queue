using Microsoft.Extensions.Logging;

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

    public static PsqlContextFactory CreateContextFactory()
    {
        return new PsqlContextFactory(TestOptions.Instance.PgConnectionString, TestOptions.Instance.PgVersion,
            optionsBuilder => optionsBuilder.LogTo(Console.Error.WriteLine, LogLevel.Information));
    }
}

[CollectionDefinition("DbTest")]
public class CollectionDefinition : ICollectionFixture<TestDatabaseFixture>
{
}
