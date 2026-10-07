// ReSharper disable UseAwaitUsing
namespace FclEx.Dapper;

public partial class TruncateTests
{
    public static TheoryData<DbDriver, string?, bool> TransactionCases =>
        (from pair in GetDriverSchemaCases(Schemas)
         where pair.Driver is DbDriver.Sqlite or DbDriver.SqlServer or DbDriver.Npgsql
         from restart in new[] { false, true }
         where SupportsOptions(pair.Driver, restart, false)
         select (pair.Driver, pair.Schema, restart)).ToTheoryData();

    [Theory(DisableParallelization = true, SkipTestWithoutData = true)]
    [MemberData(nameof(TransactionCases))]
    public async Task TruncateAsync_UsesCallerTransactionAndCanRollBack(DbDriver driver, string? schema, bool restartIdentity)
    {
        SkipMySql(driver);
        using var connection = Fixture.CreateDbConnection(driver, schema);
        try
        {
            var entity = await SeedAsync(connection, schema);
            await connection.OpenAsync(CancellationToken);
            using (var transaction = await connection.BeginTransactionAsync(IsolationLevel.Serializable, CancellationToken))
            {
                var options = new CommandOptions { Transaction = transaction, TimeoutSeconds = 10 };
                await connection.TruncateAsync<EntityWithAutoKey>(restartIdentity, false, schema, options, CancellationToken);
                Assert.Equal(0, await CountAsync<EntityWithAutoKey>(connection, schema, transaction));
                Assert.Same(connection, transaction.Connection);
                await transaction.RollbackAsync(CancellationToken);
            }
            Assert.Equal(ConnectionState.Open, connection.State);
            Assert.Equal(2, await CountAsync<EntityWithAutoKey>(connection, schema));
            var nextId = await connection.InsertAsync<EntityWithAutoKey, int>(new() { Name = Guid.NewGuid().ToString(), Value = 1 },
                schema, cancellationToken: CancellationToken);
            Assert.True(nextId > entity.Id);
        }
        finally
        {
            await DeleteAllAsync<EntityWithAutoKey>(connection, schema);
        }
    }

    public static TheoryData<DbDriver> SqliteDriverCases => SelectedDrivers.Where(driver => driver == DbDriver.Sqlite).ToTheoryData();

    [Theory(DisableParallelization = true, SkipTestWithoutData = true)]
    [MemberData(nameof(SqliteDriverCases))]
    public async Task TruncateAsync_AcceptsDerivedConnectionType(DbDriver driver)
    {
        SkipMySql(driver);
        using var connection = new DerivedSqliteConnection(Fixture.DatabaseEnvironment.Resolve(driver).BuildConnectionString());
        await VerifyTruncationAsync(connection, driver, null,
            token => connection.TruncateAsync<EntityWithAutoKey>(cancellationToken: token));
    }

    private sealed class DerivedSqliteConnection(string connectionString) : SqliteConnection(connectionString);
}
