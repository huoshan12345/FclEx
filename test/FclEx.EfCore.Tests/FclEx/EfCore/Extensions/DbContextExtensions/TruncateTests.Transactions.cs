namespace FclEx.EfCore.Extensions.DbContextExtensions;

public partial class TruncateTests
{
    public static TheoryData<DbDriver> SqliteDriverCases => SelectedDrivers
        .Where(driver => driver == DbDriver.Sqlite).ToTheoryData();

    [Theory(DisableParallelization = true, SkipTestWithoutData = true)]
    [MemberData(nameof(SqliteDriverCases))]
    public async Task TruncateAsync_AcceptsDerivedConnectionType(DbDriver driver)
    {
        await using var connection = new DerivedSqliteConnection(Fixture.ResolveTarget(driver).BuildConnectionString());
        await using var context = Fixture.CreateDbContext(driver);
        context.Database.SetDbConnection(connection);
        await VerifyTruncationAsync(context, token => context.TruncateAsync<EntityWithAutoKey>(token));
    }

    private sealed class DerivedSqliteConnection(string connectionString) : SqliteConnection(connectionString);

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
            return cases;
        }
    }

    [Theory(DisableParallelization = true, SkipTestWithoutData = true)]
    [MemberData(nameof(TransactionCases))]
    public async Task TruncateAsync_UsesCallerTransactionAndCanRollBack(
        DbDriver driver, string? schema, bool restartIdentity)
    {
        await using var context = Fixture.CreateDbContext(driver, schema);
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
