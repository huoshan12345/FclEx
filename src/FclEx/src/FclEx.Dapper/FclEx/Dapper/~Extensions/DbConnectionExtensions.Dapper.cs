namespace FclEx.Dapper;

partial class DbConnectionExtensions
{
    public static async Task<T> DoTransactionAsync<T>(this IDbConnection con, Func<IDbConnection, Task<T>> action, IsolationLevel level = IsolationLevel.ReadUncommitted)
    {
        var scope = DapperHelper.CreateAsyncTransactionScope(level);
        try
        {
            var result = await action(con);
            scope.Complete();
            return result;
        }
        finally
        {
            scope.Dispose();
        }
    }

    public static async Task DoTransactionAsync(this IDbConnection con, Func<IDbConnection, Task> action, IsolationLevel level = IsolationLevel.ReadUncommitted)
    {
        var scope = DapperHelper.CreateAsyncTransactionScope(level);
        try
        {
            await action(con);
            scope.Complete();
        }
        finally
        {
            scope.Dispose();
        }
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