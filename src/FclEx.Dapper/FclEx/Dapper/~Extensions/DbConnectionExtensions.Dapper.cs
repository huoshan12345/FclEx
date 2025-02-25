namespace FclEx.Dapper;

partial class DbConnectionExtensions
{
    public static async Task<T> DoTransactionAsync<T>(this DbConnection con, Func<DbTransaction, Task<T>> action, IsolationLevel level = IsolationLevel.ReadUncommitted)
    {
        await con.TryOpenAsync();
        await using var tran = await con.BeginTransactionAsync(level);
        try
        {
            var result = await action(tran);
            await tran.CommitAsync();
            return result;
        }
        catch
        {
            await tran.TryRollbackAsync();
            throw;
        }
    }

    public static async Task DoTransactionAsync(this DbConnection con, Func<DbTransaction, Task> action, IsolationLevel level = IsolationLevel.ReadUncommitted)
    {
        await con.TryOpenAsync();
        await using var tran = await con.BeginTransactionAsync(level);
        try
        {
            await action(tran);
            await tran.CommitAsync();
        }
        catch
        {
            await tran.TryRollbackAsync();
            throw;
        }
    }

    public static async Task DoTransactionAsync(this IReadOnlyList<DbConnection> cons, Func<IReadOnlyList<DbTransaction>, Task> action, IsolationLevel level = IsolationLevel.ReadUncommitted)
    {
        var trans = await cons.Select(m => m.BeginTransactionAsync(level)).WhenAll();
        try
        {
            await action(trans);
            foreach (var tran in trans)
                await tran.CommitAsync();
        }
        catch (Exception ex)
        {
            await trans.TryRollbackAsync(ex);
        }
        finally
        {
            foreach (var tran in trans)
                await tran.DisposeAsync();
        }
    }

    public static Task TryOpenAsync(this DbConnection con, CancellationToken token = default)
    {
        if (con.State == ConnectionState.Open)
            return Task.CompletedTask;

        return con.OpenAsync(token);
    }


#if NETSTANDARD2_0
    internal static Task<DbTransactionWrapper> BeginTransactionAsync(this DbConnection con, IsolationLevel isolationLevel)
    {
        var tran = con.BeginTransaction(isolationLevel);
        return new DbTransactionWrapper(tran).ToTask();
    }

    internal class DbTransactionWrapper : DbTransaction, IAsyncDisposable
    {
        private readonly DbTransaction _transaction;

        public DbTransactionWrapper(DbTransaction connection)
        {
            _transaction = connection is DbTransactionWrapper wrapper
                ? wrapper._transaction
                : connection;
        }

        public ValueTask DisposeAsync()
        {
            _transaction.Dispose();
            return new(Task.CompletedTask);
        }

        public override void Commit()
        {
            _transaction.Commit();
        }

        public override void Rollback()
        {
            _transaction.Rollback();
        }

        protected override DbConnection DbConnection => _transaction.Connection;
        public override IsolationLevel IsolationLevel => _transaction.IsolationLevel;
    }
#endif
}