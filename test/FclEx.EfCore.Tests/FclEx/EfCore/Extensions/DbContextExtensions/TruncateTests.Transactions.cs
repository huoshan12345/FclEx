namespace FclEx.EfCore.Extensions.DbContextExtensions;

public partial class TruncateTests
{
    [Theory(SkipTestWithoutData = true)]
    [MemberData(nameof(TruncateTestCases.SqliteDriverCases), MemberType = typeof(TruncateTestCases))]
    public async Task TruncateAsync_AcceptsDerivedConnectionType(DbDriver driver)
    {
        Assert.SkipMySql(driver);

        using var session = await Fixture.TruncateTables.CreateSessionAsync(driver, null, [typeof(TruncateRow)],
            new DerivedSqliteConnection(Fixture.ResolveTarget(driver).BuildConnectionString()), CancellationToken);
        await using var context = session.CreateDbContext(Fixture, null);
        await using var cleanup = CleanupRows(context);
        await VerifyRowsRemovedAsync(context, token => context.TruncateAsync<TruncateRow>(token));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task TruncateAsync_TemporarySqliteTableResetsItsOwnSequence(bool hasMainTable)
    {
        await using var context = new TruncateDbContext(DbDriver.Sqlite, "Data Source=:memory:;Foreign Keys=True");
        await context.Database.OpenConnectionAsync(CancellationToken);
        if (hasMainTable)
            await context.Database.ExecuteSqlRawAsync("""
                CREATE TABLE TruncateRow (Id INTEGER PRIMARY KEY AUTOINCREMENT, Value INTEGER NOT NULL);
                INSERT INTO main.TruncateRow (Id, Value) VALUES (8, 1);
                """, CancellationToken);
        await context.Database.ExecuteSqlRawAsync("""
            CREATE TEMP TABLE TruncateRow (Id INTEGER PRIMARY KEY AUTOINCREMENT, Value INTEGER NOT NULL);
            INSERT INTO temp.TruncateRow (Value) VALUES (1), (2);
            """, CancellationToken);
        await context.TruncateAsync<TruncateRow>(CancellationToken);
        Assert.Equal(0, await context.TruncateRow.CountAsync(CancellationToken));
        var next = new TruncateRow { Value = 3 };
        context.TruncateRow.Add(next);
        await context.SaveChangesAsync(CancellationToken);
        Assert.Equal(1, next.Id);
        if (hasMainTable)
        {
            Assert.Equal(1, await context.Database.SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM main.TruncateRow").SingleAsync(CancellationToken));
            Assert.Equal(8, await context.Database.SqlQueryRaw<int>("SELECT seq AS Value FROM main.sqlite_sequence WHERE name = 'TruncateRow'").SingleAsync(CancellationToken));
        }
    }

    public static TheoryData<DbDriver, string?, bool> TransactionCases => TruncateTestCases.GetTransactionCases(Schemas);

    [Theory(SkipTestWithoutData = true)]
    [MemberData(nameof(TransactionCases))]
    public async Task TruncateAsync_UsesCallerTransactionAndCanRollBack(
        DbDriver driver, string? schema, bool restartIdentity)
    {
        Assert.SkipMySql(driver);

        using var session = await Fixture.TruncateTables.CreateSessionAsync(driver, schema, [typeof(TruncateRow)], cancellationToken: CancellationToken);
        await using var context = session.CreateDbContext(Fixture, schema);
        try
        {
            var tracked = await SeedAsync(context);
            var previousId = tracked.Id;
            await using (var transaction = await context.Database.BeginTransactionAsync(CancellationToken))
            {
                await context.TruncateAsync<TruncateRow>(restartIdentity, false, CancellationToken);
                Assert.Equal(0, await context.TruncateRow.CountAsync(CancellationToken));
                await transaction.RollbackAsync(CancellationToken);
            }
            Assert.Equal(2, await context.TruncateRow.CountAsync(CancellationToken));
            context.ChangeTracker.Clear();
            var next = new TruncateRow { Value = 1 };
            context.TruncateRow.Add(next);
            await context.SaveChangesAsync(CancellationToken);
            Assert.True(next.Id > previousId);
        }
        finally
        {
            await context.TruncateRow.ExecuteDeleteAsync();
        }
    }
}
