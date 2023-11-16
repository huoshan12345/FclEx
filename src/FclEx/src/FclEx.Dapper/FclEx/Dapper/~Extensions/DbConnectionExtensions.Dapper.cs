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

    public static async Task DoTransactionAsync(this IList<DbConnection> cons, Func<IList<DbTransaction>, Task> action,
        IsolationLevel level = IsolationLevel.ReadUncommitted)
    {
        var trans = await cons.Select(m => m.BeginTransactionAsync(level)).WhenAll();
        try
        {
            await action(trans).IgnoreSyncContext();
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
}