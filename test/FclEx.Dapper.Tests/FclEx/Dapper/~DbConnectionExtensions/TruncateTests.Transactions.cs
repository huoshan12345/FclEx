// ReSharper disable UseAwaitUsing
namespace FclEx.Dapper;

public partial class TruncateTests
{
    public static TheoryData<DbDriver, string?, bool> TransactionCases
    {
        get
        {
            var cases = (from pair in GetDriverSchemaCases(Schemas)
                         where pair.Driver is DbDriver.Sqlite or DbDriver.SqlServer or DbDriver.Npgsql
                         from restart in new[] { false, true }
                         where SupportsOptions(pair.Driver, restart, false)
                         select (pair.Driver, pair.Schema, restart)).ToTheoryData();
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
    public async Task TruncateAsync_UsesCallerTransactionAndCanRollBack(DbDriver driver, string? schema, bool restartIdentity)
    {
        Assert.SkipMySql(driver);

        using var session = await CreateSessionAsync(driver, schema);
        var connection = session.Connection;
        schema = session.Schema;
        try
        {
            var entity = await SeedAsync(session);
            using (var transaction = await connection.BeginTransactionAsync(IsolationLevel.Serializable, CancellationToken))
            {
                var options = session.CommandOptions with { Transaction = transaction, TimeoutSeconds = 10 };
                await connection.TruncateAsync<EntityWithAutoKey>(restartIdentity, false, schema, options, CancellationToken);
                Assert.Equal(0, await CountAsync<EntityWithAutoKey>(session, transaction));
                Assert.Same(connection, transaction.Connection);
                await transaction.RollbackAsync(CancellationToken);
            }
            Assert.Equal(ConnectionState.Open, connection.State);
            Assert.Equal(2, await CountAsync<EntityWithAutoKey>(session));
            var nextId = await connection.InsertAsync<EntityWithAutoKey, int>(new() { Name = Guid.NewGuid().ToString(), Value = 1 },
                schema, commandOptions: session.CommandOptions, cancellationToken: CancellationToken);
            Assert.True(nextId > entity.Id);
        }
        finally
        {
            await DeleteAllAsync<EntityWithAutoKey>(session);
        }
    }

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
        Assert.SkipMySql(driver);

        using var session = await Fixture.TruncateTables.CreateSessionAsync(driver, null, [typeof(EntityWithAutoKey)],
            new DerivedSqliteConnection(Fixture.DatabaseEnvironment.Resolve(driver).BuildConnectionString()), CancellationToken);
        var connection = session.Connection;
        await VerifyTruncationAsync(session,
            token => connection.TruncateAsync<EntityWithAutoKey>(commandOptions: session.CommandOptions, cancellationToken: token));
    }

    private sealed class DerivedSqliteConnection(string connectionString) : SqliteConnection(connectionString);
}
