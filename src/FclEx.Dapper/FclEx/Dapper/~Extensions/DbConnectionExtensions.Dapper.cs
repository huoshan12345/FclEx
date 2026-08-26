namespace FclEx.Dapper;

partial class DbConnectionExtensions
{
    /// <summary>
    /// Executes an asynchronous callback in a local database transaction and commits it when the callback succeeds.
    /// </summary>
    /// <typeparam name="T">The callback result type.</typeparam>
    /// <param name="con">The connection on which the transaction is created.</param>
    /// <param name="action">The work executed inside the transaction.</param>
    /// <param name="level">The transaction isolation level.</param>
    /// <param name="cancellationToken">The token used to cancel opening, transaction creation, and commit.</param>
    /// <returns>The callback result.</returns>
    public static Task<T> DoTransactionAsync<T>(
        this DbConnection con,
        Func<DbTransaction, Task<T>> action,
        IsolationLevel level = IsolationLevel.ReadUncommitted,
        CancellationToken cancellationToken = default)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        return con.DoTransactionAsync((transaction, _) => action(transaction), level, cancellationToken);
    }

    /// <summary>
    /// Executes a cancellable asynchronous callback in a local database transaction and commits it when the callback succeeds.
    /// </summary>
    /// <typeparam name="T">The callback result type.</typeparam>
    /// <param name="con">The connection on which the transaction is created.</param>
    /// <param name="action">The work executed inside the transaction. It receives the operation cancellation token.</param>
    /// <param name="level">The transaction isolation level.</param>
    /// <param name="cancellationToken">The token used for the complete transaction operation.</param>
    /// <returns>The callback result.</returns>
    public static async Task<T> DoTransactionAsync<T>(
        this DbConnection con,
        Func<DbTransaction, CancellationToken, Task<T>> action,
        IsolationLevel level = IsolationLevel.ReadUncommitted,
        CancellationToken cancellationToken = default)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        await con.TryOpenAsync(cancellationToken);
#if NET5_0_OR_GREATER
        await
#endif
        using var tran = await con.BeginTransactionAsync(level, cancellationToken);
        try
        {
            var result = await action(tran, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            await tran.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            // Rollback is cleanup: once cancellation has been requested, using the cancelled token would skip it.
            var rollbackToken = cancellationToken.IsCancellationRequested
                ? CancellationToken.None
                : cancellationToken;
            await tran.TryRollbackAsync(rollbackToken);
            throw;
        }
    }

    /// <summary>
    /// Executes an asynchronous callback in a local database transaction and commits it when the callback succeeds.
    /// </summary>
    /// <param name="con">The connection on which the transaction is created.</param>
    /// <param name="action">The work executed inside the transaction.</param>
    /// <param name="level">The transaction isolation level.</param>
    /// <param name="cancellationToken">The token used to cancel opening, transaction creation, and commit.</param>
    /// <returns>A task representing the transaction operation.</returns>
    public static Task DoTransactionAsync(
        this DbConnection con,
        Func<DbTransaction, Task> action,
        IsolationLevel level = IsolationLevel.ReadUncommitted,
        CancellationToken cancellationToken = default)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        return con.DoTransactionAsync((transaction, _) => action(transaction), level, cancellationToken);
    }

    /// <summary>
    /// Executes a cancellable asynchronous callback in a local database transaction and commits it when the callback succeeds.
    /// </summary>
    /// <param name="con">The connection on which the transaction is created.</param>
    /// <param name="action">The work executed inside the transaction. It receives the operation cancellation token.</param>
    /// <param name="level">The transaction isolation level.</param>
    /// <param name="cancellationToken">The token used for the complete transaction operation.</param>
    /// <returns>A task representing the transaction operation.</returns>
    public static async Task DoTransactionAsync(
        this DbConnection con,
        Func<DbTransaction, CancellationToken, Task> action,
        IsolationLevel level = IsolationLevel.ReadUncommitted,
        CancellationToken cancellationToken = default)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        await con.DoTransactionAsync<int>(async (transaction, token) =>
        {
            await action(transaction, token);
            return 0;
        }, level, cancellationToken);
    }

    /// <summary>
    /// Opens a connection when it is not already open.
    /// </summary>
    /// <param name="con">The connection to open.</param>
    /// <param name="cancellationToken">The token used to cancel connection opening.</param>
    /// <returns>A task representing the open operation.</returns>
    public static Task TryOpenAsync(this DbConnection con, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (con.State == ConnectionState.Open)
            return Task.CompletedTask;

        return con.OpenAsync(cancellationToken);
    }
}
