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
                var options = new CommandOptions { Transaction = transaction, TimeoutSeconds = 10 };
                await connection.TruncateAsync<TruncateRow>(restartIdentity, false, schema, options, CancellationToken);
                Assert.Equal(0, await CountAsync<TruncateRow>(session, transaction));
                Assert.Same(connection, transaction.Connection);
                await transaction.RollbackAsync(CancellationToken);
            }
            Assert.Equal(ConnectionState.Open, connection.State);
            Assert.Equal(2, await CountAsync<TruncateRow>(session));
            var nextId = await connection.InsertAsync<TruncateRow, int>(new() { Value = 1 },
                schema, cancellationToken: CancellationToken);
            Assert.True(nextId > entity.Id);
        }
        finally
        {
            await DeleteAllAsync<TruncateRow>(session);
        }
    }

    [Theory(SkipTestWithoutData = true)]
    [MemberData(nameof(TruncateTestCases.SqliteDriverCases), MemberType = typeof(TruncateTestCases))]
    public async Task TruncateAsync_AcceptsDerivedConnectionType(DbDriver driver)
    {
        Assert.SkipMySql(driver);

        using var session = await Fixture.TruncateSessions.CreateSessionAsync(driver, null, [typeof(TruncateRow)],
            new DerivedSqliteConnection(Fixture.DatabaseEnvironment.Resolve(driver).BuildConnectionString()), CancellationToken);
        await using var cleanup = CleanupRows(session);
        var connection = session.Connection;
        await VerifyRowsRemovedAsync(session,
            token => connection.TruncateAsync<TruncateRow>(cancellationToken: token));
    }
}
