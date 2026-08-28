namespace FclEx.Dapper;

partial class DbConnectionExtensions
{
    /// <summary>
    /// Executes an asynchronous callback in a local database transaction and commits it when the callback succeeds.
    /// </summary>
    /// <typeparam name="T">The callback result type.</typeparam>
    /// <param name="connection">The connection on which the transaction is created. A connection opened here is closed before return.</param>
    /// <param name="action">The work executed inside the transaction.</param>
    /// <param name="level">The transaction isolation level. The default is <see cref="IsolationLevel.ReadCommitted"/>.</param>
    /// <param name="cancellationToken">The token used to cancel opening, transaction creation, and commit.</param>
    /// <returns>The callback result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="action"/> is <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> is cancelled before the transaction commits.</exception>
    /// <exception cref="AggregateException">The operation or commit failed and rollback also failed. Both exceptions are available in <see cref="AggregateException.InnerExceptions"/>.</exception>
    public static Task<T> ExecuteInTransactionAsync<T>(
        this DbConnection connection,
        Func<DbTransaction, Task<T>> action,
        IsolationLevel level = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        return connection.ExecuteInTransactionAsync((transaction, _) => action(transaction), level, cancellationToken);
    }

    /// <summary>
    /// Executes a cancellable asynchronous callback in a local database transaction and commits it when the callback succeeds.
    /// </summary>
    /// <typeparam name="T">The callback result type.</typeparam>
    /// <param name="connection">The connection on which the transaction is created. A connection opened here is closed before return.</param>
    /// <param name="action">The work executed inside the transaction. It receives the operation cancellation token.</param>
    /// <param name="level">The transaction isolation level. The default is <see cref="IsolationLevel.ReadCommitted"/>.</param>
    /// <param name="cancellationToken">The token used for the complete transaction operation.</param>
    /// <returns>The callback result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="action"/> is <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> is cancelled before the transaction commits.</exception>
    /// <exception cref="AggregateException">The operation or commit failed and rollback also failed. Both exceptions are available in <see cref="AggregateException.InnerExceptions"/>.</exception>
    public static async Task<T> ExecuteInTransactionAsync<T>(
        this DbConnection connection,
        Func<DbTransaction, CancellationToken, Task<T>> action,
        IsolationLevel level = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        var initialState = connection.State;
        try
        {
            await connection.TryOpenAsync(cancellationToken);
#if NET5_0_OR_GREATER
            await
#endif
            using var tran = await connection.BeginTransactionAsync(level, cancellationToken);
            try
            {
                var result = await action(tran, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                await tran.CommitAsync(cancellationToken);
                return result;
            }
            catch (Exception operationException)
            {
                // Rollback is cleanup: once cancellation has been requested, using the cancelled token would skip it.
                var rollbackToken = cancellationToken.IsCancellationRequested
                    ? CancellationToken.None
                    : cancellationToken;
                try
                {
                    await tran.TryRollbackAsync(rollbackToken);
                }
                catch (Exception rollbackException)
                {
                    throw new AggregateException(
                        "The transaction operation failed and rollback also failed.",
                        operationException,
                        rollbackException);
                }

                throw;
            }
        }
        finally
        {
            RestoreInitialConnectionState(connection, initialState);
        }
    }

    /// <summary>
    /// Executes an asynchronous callback in a local database transaction and commits it when the callback succeeds.
    /// </summary>
    /// <param name="connection">The connection on which the transaction is created. A connection opened here is closed before return.</param>
    /// <param name="action">The work executed inside the transaction.</param>
    /// <param name="level">The transaction isolation level. The default is <see cref="IsolationLevel.ReadCommitted"/>.</param>
    /// <param name="cancellationToken">The token used to cancel opening, transaction creation, and commit.</param>
    /// <returns>A task representing the transaction operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="action"/> is <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> is cancelled before the transaction commits.</exception>
    /// <exception cref="AggregateException">The operation or commit failed and rollback also failed. Both exceptions are available in <see cref="AggregateException.InnerExceptions"/>.</exception>
    public static Task ExecuteInTransactionAsync(
        this DbConnection connection,
        Func<DbTransaction, Task> action,
        IsolationLevel level = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        return connection.ExecuteInTransactionAsync((transaction, _) => action(transaction), level, cancellationToken);
    }

    /// <summary>
    /// Executes a cancellable asynchronous callback in a local database transaction and commits it when the callback succeeds.
    /// </summary>
    /// <param name="connection">The connection on which the transaction is created. A connection opened here is closed before return.</param>
    /// <param name="action">The work executed inside the transaction. It receives the operation cancellation token.</param>
    /// <param name="level">The transaction isolation level. The default is <see cref="IsolationLevel.ReadCommitted"/>.</param>
    /// <param name="cancellationToken">The token used for the complete transaction operation.</param>
    /// <returns>A task representing the transaction operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="action"/> is <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> is cancelled before the transaction commits.</exception>
    /// <exception cref="AggregateException">The operation or commit failed and rollback also failed. Both exceptions are available in <see cref="AggregateException.InnerExceptions"/>.</exception>
    public static async Task ExecuteInTransactionAsync(
        this DbConnection connection,
        Func<DbTransaction, CancellationToken, Task> action,
        IsolationLevel level = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        await connection.ExecuteInTransactionAsync<int>(async (transaction, token) =>
        {
            await action(transaction, token);
            return 0;
        }, level, cancellationToken);
    }

    /// <summary>
    /// Opens a connection when it is not already open.
    /// </summary>
    /// <param name="connection">The connection to open.</param>
    /// <param name="cancellationToken">The token used to cancel connection opening.</param>
    /// <returns>A task representing the open operation.</returns>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> is cancelled before the connection opens.</exception>
    public static Task TryOpenAsync(this DbConnection connection, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (connection.State == ConnectionState.Open)
            return Task.CompletedTask;

        return connection.OpenAsync(cancellationToken);
    }
}
