namespace Rz.TaskQueue.Test;

[Collection("DbTest")]
public abstract class DbTest : IDisposable
{
    protected TestDatabaseFixture DbFixture {  get; set; }

    //protected PsqlContext InitContext {  get; set; }

    protected DbTest(TestDatabaseFixture fixture)
    {
        DbFixture = fixture;
        //InitContext = DbFixture.CreateContext();
        //InitContext.Database.BeginTransaction();
    }

    public void Dispose()
    {
        //InitContext.ChangeTracker.Clear();
        //InitContext.Dispose();
    }
}
