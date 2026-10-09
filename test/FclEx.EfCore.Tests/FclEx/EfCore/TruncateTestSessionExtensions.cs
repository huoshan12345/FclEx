namespace FclEx.EfCore;

internal static class TruncateTestSessionExtensions
{
    public static TestDbContext CreateDbContext(this TruncateTestSession session, EfCoreFixture fixture)
    {
        var context = fixture.CreateDbContext(session.Driver, session.Schema);
        context.Database.SetDbConnection(session.Connection);
        return context;
    }

    public static TestDbContext CreateSharedModelContext(this TestDbContext reference)
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseModel(reference.Model);
        var context = new TestDbContext(reference.DbDriver, reference.ConnectionString, reference.Schema, options.Options);
        context.Database.SetDbConnection(reference.Database.GetDbConnection());
        return context;
    }
}
