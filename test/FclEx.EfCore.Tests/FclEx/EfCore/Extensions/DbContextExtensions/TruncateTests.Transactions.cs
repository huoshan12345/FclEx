namespace FclEx.EfCore.Extensions.DbContextExtensions;

public partial class TruncateTests
{
    public static TheoryData<DbDriver> SqliteDriverCases
    {
        get
        {
            var cases = SelectedDrivers.Where(driver => driver == DbDriver.Sqlite).ToTheoryData();
            if (cases.Count == 0)
                cases.Add(new TheoryDataRow<DbDriver>(DbDriver.Sqlite) { Skip = "SQLite is not selected." });
            return cases;
        }
    }

    [Theory(SkipTestWithoutData = true)]
    [MemberData(nameof(SqliteDriverCases))]
    public async Task TruncateAsync_AcceptsDerivedConnectionType(DbDriver driver)
    {
        Assert.SkipWhen(driver == DbDriver.MySql, "MySql.Data asynchronous timeout cleanup can hang.");
        using var session = await Fixture.TruncateTables.CreateSessionAsync(driver, null, [typeof(EntityWithAutoKey)], CancellationToken,
            new DerivedSqliteConnection(Fixture.ResolveTarget(driver).BuildConnectionString()));
        await using var context = session.CreateDbContext(Fixture, null);
        await VerifyTruncationAsync(context, token => context.TruncateAsync<EntityWithAutoKey>(token));
    }

    private sealed class DerivedSqliteConnection(string connectionString) : SqliteConnection(connectionString);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task TruncateAsync_TemporarySqliteTableResetsItsOwnSequence(bool hasMainTable)
    {
        await using var context = new TestDbContext(DbDriver.Sqlite, "Data Source=:memory:;Foreign Keys=True");
        await context.Database.OpenConnectionAsync(CancellationToken);
        if (hasMainTable)
            await context.Database.ExecuteSqlRawAsync("""
                CREATE TABLE EntityWithAutoKey (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT, Value INTEGER NOT NULL);
                INSERT INTO main.EntityWithAutoKey (Id, Value) VALUES (8, 1);
                """, CancellationToken);
        await context.Database.ExecuteSqlRawAsync("""
            CREATE TEMP TABLE EntityWithAutoKey (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT, Value INTEGER NOT NULL);
            INSERT INTO temp.EntityWithAutoKey (Value) VALUES (1), (2);
            """, CancellationToken);
        await context.TruncateAsync<EntityWithAutoKey>(CancellationToken);
        Assert.Equal(0, await context.EntityWithAutoKey.CountAsync(CancellationToken));
        var next = new EntityWithAutoKey { Name = "after", Value = 3 };
        context.EntityWithAutoKey.Add(next);
        await context.SaveChangesAsync(CancellationToken);
        Assert.Equal(1, next.Id);
        if (hasMainTable)
        {
            Assert.Equal(1, await context.Database.SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM main.EntityWithAutoKey").SingleAsync(CancellationToken));
            Assert.Equal(8, await context.Database.SqlQueryRaw<int>("SELECT seq AS Value FROM main.sqlite_sequence WHERE name = 'EntityWithAutoKey'").SingleAsync(CancellationToken));
        }
    }

    public static TheoryData<DbDriver, string?, bool> TransactionCases
    {
        get
        {
            var cases = new TheoryData<DbDriver, string?, bool>();
            foreach (var (driver, schema) in GetDriverSchemaCases(Schemas))
            {
                if (driver is not (DbDriver.Sqlite or DbDriver.SqlServer or DbDriver.Npgsql))
                    continue;
                foreach (var restart in new[] { false, true })
                    if (SupportsOptions(driver, restart, false))
                        cases.Add(driver, schema, restart);
            }
            if (cases.Count == 0)
                cases.Add(new TheoryDataRow<DbDriver, string?, bool>(SelectedDrivers.FirstOrDefault(), null, true)
                {
                    Skip = "No selected driver supports rolling back TRUNCATE.",
                });
            return cases;
        }
    }

    [Theory(SkipTestWithoutData = true)]
    [MemberData(nameof(TransactionCases))]
    public async Task TruncateAsync_UsesCallerTransactionAndCanRollBack(
        DbDriver driver, string? schema, bool restartIdentity)
    {
        Assert.SkipWhen(driver == DbDriver.MySql, "MySql.Data asynchronous timeout cleanup can hang.");
        using var session = await Fixture.TruncateTables.CreateSessionAsync(driver, schema, [typeof(EntityWithAutoKey)], CancellationToken);
        await using var context = session.CreateDbContext(Fixture, schema);
        try
        {
            var tracked = await SeedAsync(context);
            var previousId = tracked.Id;
            await using (var transaction = await context.Database.BeginTransactionAsync(CancellationToken))
            {
                await context.TruncateAsync<EntityWithAutoKey>(restartIdentity, false, CancellationToken);
                Assert.Equal(0, await context.EntityWithAutoKey.CountAsync(CancellationToken));
                await transaction.RollbackAsync(CancellationToken);
            }
            Assert.Equal(2, await context.EntityWithAutoKey.CountAsync(CancellationToken));
            context.ChangeTracker.Clear();
            var next = new EntityWithAutoKey { Name = Guid.NewGuid().ToString(), Value = 1 };
            context.EntityWithAutoKey.Add(next);
            await context.SaveChangesAsync(CancellationToken);
            Assert.True(next.Id > previousId);
        }
        finally
        {
            await context.EntityWithAutoKey.ExecuteDeleteAsync();
        }
    }
}
