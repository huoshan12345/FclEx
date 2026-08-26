namespace FclEx.Dapper;

public static class DbTransactionExtensions
{
#if !NET5_0_OR_GREATER
    /// <summary>
    /// Commits the transaction after checking for cancellation on target frameworks without native async commit.
    /// </summary>
    /// <param name="tran">The transaction to commit.</param>
    /// <param name="cancellationToken">The token checked before the synchronous commit begins.</param>
    /// <returns>A completed task after the commit finishes.</returns>
    public static Task CommitAsync(this DbTransaction tran, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        tran.Commit();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Rolls back the transaction after checking for cancellation on target frameworks without native async rollback.
    /// </summary>
    /// <param name="tran">The transaction to roll back.</param>
    /// <param name="cancellationToken">The token checked before the synchronous rollback begins.</param>
    /// <returns>A completed task after the rollback finishes.</returns>
    public static Task RollbackAsync(this DbTransaction tran, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        tran.Rollback();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Begins a transaction after checking for cancellation on target frameworks without native asynchronous creation.
    /// </summary>
    /// <param name="con">The connection on which the transaction is created.</param>
    /// <param name="level">The transaction isolation level.</param>
    /// <param name="cancellationToken">The token checked before transaction creation begins.</param>
    /// <returns>The created transaction.</returns>
    public static Task<DbTransaction> BeginTransactionAsync(
        this DbConnection con,
        IsolationLevel level = IsolationLevel.ReadUncommitted,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(con.BeginTransaction(level));
    }

    /// <summary>
    /// Asynchronously compatible disposal for target frameworks without native asynchronous transaction disposal.
    /// </summary>
    /// <param name="tran">The transaction to dispose.</param>
    /// <returns>A completed value task after synchronous disposal.</returns>
    public static ValueTask DisposeAsync(this DbTransaction tran)
    {
        tran.Dispose();
        return ValueTask.CompletedTask;
    }
#endif

    /// <summary>
    /// Rolls back the transaction when it is still associated with an open connection.
    /// </summary>
    /// <param name="tran">The transaction to roll back.</param>
    /// <param name="cancellationToken">The token used to cancel rollback.</param>
    /// <returns>A task representing rollback, or a completed task when rollback is no longer possible.</returns>
    public static async Task TryRollbackAsync(this DbTransaction tran, CancellationToken cancellationToken = default)
    {
        if (tran.Connection is not { State: ConnectionState.Open })
            return;

        await tran.RollbackAsync(cancellationToken);
    }

    /// <summary>
    /// Inserts one mapped entity through this transaction and optionally returns its generated key.
    /// </summary>
    /// <typeparam name="TEntity">The mapped entity type.</typeparam>
    /// <typeparam name="TKey">The generated-key type requested by the caller.</typeparam>
    /// <param name="tran">The transaction assigned to the insert command.</param>
    /// <param name="entity">The entity whose mapped values are inserted.</param>
    /// <param name="schema">An optional schema overriding the schema in the entity mapping.</param>
    /// <param name="returnId">Whether to return the single generated key when one is mapped.</param>
    /// <param name="includeAutoKey">Whether to insert a mapped generated key explicitly.</param>
    /// <param name="timeoutSeconds">The optional command timeout in seconds.</param>
    /// <param name="sqlAdapter">An optional SQL adapter overriding connection-type resolution.</param>
    /// <param name="cancellationToken">The token used to cancel command execution.</param>
    /// <returns>The generated key converted to <typeparamref name="TKey"/> when requested and supported; otherwise the default value.</returns>
    public static Task<TKey?> InsertAsync<TEntity, TKey>(
        this DbTransaction tran, 
        TEntity entity,
        string? schema = null, 
        bool returnId = true, 
        bool includeAutoKey = false,
        int? timeoutSeconds = null,
        ISqlAdapter? sqlAdapter = null,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        return tran.Connection!.InsertAsync<TEntity, TKey>(
            entity,
            schema,
            returnId,
            includeAutoKey,
            new(
                TimeoutSeconds: timeoutSeconds,
                Transaction: tran,
                SqlAdapter: sqlAdapter,
                CancellationToken: cancellationToken));
    }

    /// <summary>
    /// Inserts entities into table asynchronously and returns affected rows.
    /// </summary>
    /// <typeparam name="T">The mapped entity type.</typeparam>
    /// <param name="tran">The transaction assigned to every insert command.</param>
    /// <param name="entities">The entities to insert.</param>
    /// <param name="schema">An optional schema overriding the schema in the entity mapping.</param>
    /// <param name="includeAutoKey">Whether to insert mapped generated keys explicitly.</param>
    /// <param name="timeoutSeconds">The optional command timeout in seconds.</param>
    /// <param name="sqlAdapter">An optional SQL adapter overriding connection-type resolution.</param>
    /// <param name="cancellationToken">The token used to cancel command execution.</param>
    /// <returns>The total affected rows reported by all batches, or zero for an empty collection.</returns>
    public static Task<int> BulkInsertAsync<T>(
        this DbTransaction tran,
        IReadOnlyCollection<T> entities,
        string? schema = null, 
        bool includeAutoKey = false, 
        int? timeoutSeconds = null,
        ISqlAdapter? sqlAdapter = null,
        CancellationToken cancellationToken = default)
        where T : class
    {
        return tran.Connection!.BulkInsertAsync(
            entities,
            schema,
            includeAutoKey,
            new(
                TimeoutSeconds: timeoutSeconds,
                Transaction: tran,
                SqlAdapter: sqlAdapter,
                CancellationToken: cancellationToken));
    }

    /// <summary>
    /// Gets one mapped entity by its single key through this transaction.
    /// </summary>
    /// <typeparam name="T">The mapped entity type.</typeparam>
    /// <param name="tran">The transaction assigned to the query.</param>
    /// <param name="id">The key value to find.</param>
    /// <param name="schema">An optional schema overriding the schema in the entity mapping.</param>
    /// <param name="timeoutSeconds">The optional command timeout in seconds.</param>
    /// <param name="sqlAdapter">An optional SQL adapter overriding connection-type resolution.</param>
    /// <param name="cancellationToken">The token used to cancel command execution.</param>
    /// <returns>The matching entity, or <see langword="null"/> when no row matches.</returns>
    public static Task<T?> GetAsync<T>(
        this DbTransaction tran,
        object id,
        string? schema = null,
        int? timeoutSeconds = null,
        ISqlAdapter? sqlAdapter = null,
        CancellationToken cancellationToken = default)
    {
        return tran.Connection!.GetAsync<T>(
            id,
            schema,
            new(
                TimeoutSeconds: timeoutSeconds,
                Transaction: tran,
                SqlAdapter: sqlAdapter,
                CancellationToken: cancellationToken));
    }

    /// <summary>
    /// Deletes one mapped entity by its single key through this transaction.
    /// </summary>
    /// <typeparam name="T">The mapped entity type.</typeparam>
    /// <param name="tran">The transaction assigned to the delete command.</param>
    /// <param name="id">The key value to delete.</param>
    /// <param name="schema">An optional schema overriding the schema in the entity mapping.</param>
    /// <param name="timeoutSeconds">The optional command timeout in seconds.</param>
    /// <param name="sqlAdapter">An optional SQL adapter overriding connection-type resolution.</param>
    /// <param name="cancellationToken">The token used to cancel command execution.</param>
    /// <returns>The affected row count.</returns>
    public static Task<int> DeleteAsync<T>(
        this DbTransaction tran,
        object id,
        string? schema = null,
        int? timeoutSeconds = null,
        ISqlAdapter? sqlAdapter = null,
        CancellationToken cancellationToken = default)
    {
        return tran.Connection!.DeleteAsync<T>(
            id,
            schema,
            new(
                TimeoutSeconds: timeoutSeconds,
                Transaction: tran,
                SqlAdapter: sqlAdapter,
                CancellationToken: cancellationToken));
    }
}
