namespace FclEx.Dapper;

public static class DbTransactionExtensions
{
#if !NET5_0_OR_GREATER
    /// <summary>
    /// Commits the transaction after checking for cancellation on target frameworks without native async commit.
    /// </summary>
    /// <param name="transaction">The transaction to commit.</param>
    /// <param name="cancellationToken">The token checked before the synchronous commit begins.</param>
    /// <returns>A completed task after the commit finishes.</returns>
    public static Task CommitAsync(this DbTransaction transaction, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        transaction.Commit();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Rolls back the transaction after checking for cancellation on target frameworks without native async rollback.
    /// </summary>
    /// <param name="transaction">The transaction to roll back.</param>
    /// <param name="cancellationToken">The token checked before the synchronous rollback begins.</param>
    /// <returns>A completed task after the rollback finishes.</returns>
    public static Task RollbackAsync(this DbTransaction transaction, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        transaction.Rollback();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Begins a transaction after checking for cancellation on target frameworks without native asynchronous creation.
    /// </summary>
    /// <param name="connection">The connection on which the transaction is created.</param>
    /// <param name="level">The transaction isolation level.</param>
    /// <param name="cancellationToken">The token checked before transaction creation begins.</param>
    /// <returns>The created transaction.</returns>
    public static Task<DbTransaction> BeginTransactionAsync(
        this DbConnection connection,
        IsolationLevel level = IsolationLevel.ReadUncommitted,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(connection.BeginTransaction(level));
    }

    /// <summary>
    /// Asynchronously compatible disposal for target frameworks without native asynchronous transaction disposal.
    /// </summary>
    /// <param name="transaction">The transaction to dispose.</param>
    /// <returns>A completed value task after synchronous disposal.</returns>
    public static ValueTask DisposeAsync(this DbTransaction transaction)
    {
        transaction.Dispose();
        return ValueTask.CompletedTask;
    }
#endif

    /// <summary>
    /// Rolls back the transaction when it is still associated with an open connection.
    /// </summary>
    /// <param name="transaction">The transaction to roll back.</param>
    /// <param name="cancellationToken">The token used to cancel rollback.</param>
    /// <returns>A task representing rollback, or a completed task when rollback is no longer possible.</returns>
    public static async Task TryRollbackAsync(this DbTransaction transaction, CancellationToken cancellationToken = default)
    {
        if (transaction.Connection is not { State: ConnectionState.Open })
            return;

        await transaction.RollbackAsync(cancellationToken);
    }

    /// <summary>
    /// Inserts one mapped entity through this transaction and optionally returns its generated key.
    /// </summary>
    /// <typeparam name="TEntity">The mapped entity type.</typeparam>
    /// <typeparam name="TKey">The generated-key type requested by the caller.</typeparam>
    /// <param name="transaction">The transaction assigned to the insert command.</param>
    /// <param name="entity">The entity whose mapped values are inserted.</param>
    /// <param name="schema">An optional schema overriding the schema in the entity mapping.</param>
    /// <param name="returnGeneratedKey">Whether to return the single generated key when one is mapped.</param>
    /// <param name="commandOptions">Command execution, adapter, mapping, and cancellation options. The receiver transaction is assigned automatically.</param>
    /// <returns>The generated key converted to <typeparamref name="TKey"/> when requested and supported; otherwise the default value.</returns>
    public static Task<TKey?> InsertAsync<TEntity, TKey>(
        this DbTransaction transaction,
        TEntity entity,
        string? schema = null,
        bool returnGeneratedKey = true,
        CommandOptions commandOptions = default)
        where TEntity : class
    {
        var boundOptions = commandOptions.BindTransaction(transaction);
        return transaction.Connection!.InsertAsync<TEntity, TKey>(
            entity,
            schema,
            returnGeneratedKey,
            boundOptions);
    }

    /// <summary>
    /// Inserts one mapped entity through this transaction and returns its generated key as a long.
    /// </summary>
    /// <typeparam name="TEntity">The mapped entity type.</typeparam>
    /// <param name="transaction">The transaction assigned to the insert command.</param>
    /// <param name="entity">The entity whose mapped values are inserted.</param>
    /// <param name="schema">An optional schema overriding the schema in the entity mapping.</param>
    /// <param name="returnGeneratedKey">Whether to return the single generated key when one is mapped.</param>
    /// <param name="commandOptions">Command execution, adapter, mapping, and cancellation options. The receiver transaction is assigned automatically.</param>
    /// <returns>The generated key converted to <see langword="long"/> when requested and supported; otherwise the default value.</returns>
    public static Task<long> InsertAsync<TEntity>(
        this DbTransaction transaction,
        TEntity entity,
        string? schema = null,
        bool returnGeneratedKey = true,
        CommandOptions commandOptions = default)
        where TEntity : class
    {
        return transaction.InsertAsync<TEntity, long>(
            entity,
            schema,
            returnGeneratedKey,
            commandOptions);
    }

    /// <summary>
    /// Inserts one mapped entity through this transaction while explicitly supplying its database-generated key values.
    /// </summary>
    /// <typeparam name="TEntity">The mapped entity type.</typeparam>
    /// <param name="transaction">The transaction assigned to the insert command.</param>
    /// <param name="entity">The entity containing the generated key values to insert.</param>
    /// <param name="schema">An optional schema overriding the schema in the entity mapping.</param>
    /// <param name="commandOptions">Command execution, adapter, mapping, and cancellation options. The receiver transaction is assigned automatically.</param>
    /// <returns>A task representing the insert operation.</returns>
    /// <exception cref="DataException">The entity mapping does not contain a database-generated key.</exception>
    /// <exception cref="OperationCanceledException"><see cref="CommandOptions.CancellationToken"/> is cancelled.</exception>
    /// <remarks>
    /// This operation does not advance or reset a provider identity, sequence, or auto-increment counter.
    /// The caller must keep that state consistent so later generated keys do not conflict with the inserted values.
    /// </remarks>
    public static Task InsertWithExplicitGeneratedKeysAsync<TEntity>(
        this DbTransaction transaction,
        TEntity entity,
        string? schema = null,
        CommandOptions commandOptions = default)
        where TEntity : class
    {
        var boundOptions = commandOptions.BindTransaction(transaction);
        return transaction.Connection!.InsertWithExplicitGeneratedKeysAsync(
            entity,
            schema,
            boundOptions);
    }

    /// <summary>
    /// Inserts entities into table asynchronously and returns affected rows.
    /// </summary>
    /// <typeparam name="T">The mapped entity type.</typeparam>
    /// <param name="transaction">The transaction assigned to every insert command.</param>
    /// <param name="entities">The entities to insert.</param>
    /// <param name="schema">An optional schema overriding the schema in the entity mapping.</param>
    /// <param name="includeAutoKey">Whether to insert mapped generated keys explicitly.</param>
    /// <param name="commandOptions">Command execution, adapter, mapping, and cancellation options. The receiver transaction is assigned automatically.</param>
    /// <returns>The total affected rows reported by all batches, or zero for an empty collection.</returns>
    /// <remarks>
    /// When <paramref name="includeAutoKey"/> is <see langword="true"/>, this operation does not advance or reset a
    /// provider identity, sequence, or auto-increment counter. The caller must keep that state consistent so later
    /// generated keys do not conflict with the inserted values.
    /// </remarks>
    public static Task<int> BulkInsertAsync<T>(
        this DbTransaction transaction,
        IReadOnlyCollection<T> entities,
        string? schema = null,
        bool includeAutoKey = false,
        CommandOptions commandOptions = default)
        where T : class
    {
        var boundOptions = commandOptions.BindTransaction(transaction);
        return transaction.Connection!.BulkInsertAsync(
            entities,
            schema,
            includeAutoKey,
            boundOptions);
    }

    /// <summary>
    /// Gets one mapped entity by its single key through this transaction.
    /// </summary>
    /// <typeparam name="T">The mapped entity type.</typeparam>
    /// <param name="transaction">The transaction assigned to the query.</param>
    /// <param name="id">The key value to find.</param>
    /// <param name="schema">An optional schema overriding the schema in the entity mapping.</param>
    /// <param name="commandOptions">Command execution, adapter, mapping, and cancellation options. The receiver transaction is assigned automatically.</param>
    /// <returns>The matching entity, or <see langword="null"/> when no row matches.</returns>
    public static Task<T?> GetAsync<T>(
        this DbTransaction transaction,
        object id,
        string? schema = null,
        CommandOptions commandOptions = default)
    {
        var boundOptions = commandOptions.BindTransaction(transaction);
        return transaction.Connection!.GetAsync<T>(
            id,
            schema,
            boundOptions);
    }

    /// <summary>
    /// Deletes one mapped entity by its single key through this transaction.
    /// </summary>
    /// <typeparam name="T">The mapped entity type.</typeparam>
    /// <param name="transaction">The transaction assigned to the delete command.</param>
    /// <param name="id">The key value to delete.</param>
    /// <param name="schema">An optional schema overriding the schema in the entity mapping.</param>
    /// <param name="commandOptions">Command execution, adapter, mapping, and cancellation options. The receiver transaction is assigned automatically.</param>
    /// <returns>The affected row count.</returns>
    public static Task<int> DeleteAsync<T>(
        this DbTransaction transaction,
        object id,
        string? schema = null,
        CommandOptions commandOptions = default)
    {
        var boundOptions = commandOptions.BindTransaction(transaction);
        return transaction.Connection!.DeleteAsync<T>(
            id,
            schema,
            boundOptions);
    }
}
