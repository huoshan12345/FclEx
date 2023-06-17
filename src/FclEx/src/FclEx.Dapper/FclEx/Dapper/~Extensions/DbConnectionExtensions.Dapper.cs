namespace FclEx.Dapper;

partial class DbConnectionExtensions
{
    public static async Task<T> DoTransactionAsync<T>(this IDbConnection con, Func<IDbConnection, Task<T>> action, System.Data.IsolationLevel level = System.Data.IsolationLevel.ReadUncommitted)
    {
        using var tran = con.BeginTransaction(level);
        try
        {
            var result = await action(con);
            tran.Commit();
            return result;
        }
        catch
        {
            tran.RollbackWithCheck();
            throw;
        }
    }

    public static async Task DoTransactionAsync(this IDbConnection con, Func<IDbConnection, Task> action, System.Data.IsolationLevel level = System.Data.IsolationLevel.ReadUncommitted)
    {
        using var tran = con.BeginTransaction(level);
        try
        {
            await action(con);
            tran.Commit();
        }
        catch
        {
            tran.RollbackWithCheck();
            throw;
        }
    }

    public static void RollbackWithCheck(this IDbTransaction tran)
    {
        if (tran.Connection is not { State: ConnectionState.Open })
            return;

        tran.Rollback();
    }

    public static Task TryOpenAsync(this IDbConnection connection, CancellationToken token = default)
    {
        if (connection.State == ConnectionState.Open)
            return Task.CompletedTask;

        if (connection is DbConnection dbConn)
        {
            return dbConn.OpenAsync(token);
        }
        else
        {
            throw new InvalidOperationException("Async operations require use of a DbConnection or an already-open IDbConnection");
        }
    }
}