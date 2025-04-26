using Xunit.Abstractions;

namespace Rz.TaskQueue.Test;

[Collection("DbTest")]
public abstract class DbTest : IDisposable
{
    protected ITestOutputHelper TestOut { get; }

    protected TestDatabaseFixture DbFixture {  get; }

    protected PsqlContextFactory DbContextFactory { get; }

    //protected PsqlContext InitContext {  get; set; }

    protected DbTest(ITestOutputHelper testout, TestDatabaseFixture fixture)
    {
        TestOut = testout;
        DbFixture = fixture;
        DbContextFactory = TestDatabaseFixture.CreateContextFactory(
            optionsBuidler => optionsBuidler.LogTo(TestOut.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information));

        //InitContext = DbFixture.CreateContext();
        //InitContext.Database.BeginTransaction();
    }

    public void Dispose()
    {
        //InitContext.ChangeTracker.Clear();
        //InitContext.Dispose();
    }
}
