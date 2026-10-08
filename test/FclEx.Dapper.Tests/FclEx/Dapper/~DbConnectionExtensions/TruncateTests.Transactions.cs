// ReSharper disable UseAwaitUsing
namespace FclEx.Dapper;

public partial class TruncateTests
{
    public static TheoryData<DbDriver, string?, bool> TransactionCases => TruncateTestCases.GetTransactionCases(Schemas);

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

    [Theory(SkipTestWithoutData = true)]
    [MemberData(nameof(TruncateTestCases.SqliteDriverCases), MemberType = typeof(TruncateTestCases))]
    public async Task TruncateAsync_AcceptsDerivedConnectionType(DbDriver driver)
    {
        Assert.SkipMySql(driver);

        using var session = await Fixture.TruncateTables.CreateSessionAsync(driver, null, [typeof(EntityWithAutoKey)],
            new DerivedSqliteConnection(Fixture.DatabaseEnvironment.Resolve(driver).BuildConnectionString()), CancellationToken);
        await using var cleanup = CleanupRows(session);
        var connection = session.Connection;
        await VerifyRowsRemovedAsync(session,
            token => connection.TruncateAsync<EntityWithAutoKey>(commandOptions: session.CommandOptions, cancellationToken: token));
    }
}
