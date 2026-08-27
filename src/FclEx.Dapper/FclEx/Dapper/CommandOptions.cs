namespace FclEx.Dapper;

/// <summary>
/// Provides execution, provider, and entity-mapping options for an FclEx.Dapper command.
/// </summary>
public readonly record struct CommandOptions
{
    /// <summary>
    /// Gets the optional command timeout in seconds. Zero uses the provider's infinite-timeout convention.
    /// </summary>
    public int? TimeoutSeconds { get; init; }

    /// <summary>
    /// Gets the optional local transaction assigned to the command.
    /// </summary>
    public DbTransaction? Transaction { get; init; }

    /// <summary>
    /// Gets the optional SQL adapter overriding connection-type resolution.
    /// </summary>
    public ISqlAdapter? SqlAdapter { get; init; }

    /// <summary>
    /// Gets the optional entity mapping source. <see cref="DapperHelper.DefaultEntityMappingSource"/> is used when omitted.
    /// </summary>
    public IEntityMappingSource? EntityMappingSource { get; init; }

    /// <summary>
    /// Gets the token used to cancel connection opening and command execution.
    /// </summary>
    public CancellationToken CancellationToken { get; init; }

    /// <summary>
    /// Returns a copy of these options bound to a local transaction.
    /// </summary>
    /// <param name="transaction">The transaction to assign to the returned options.</param>
    /// <returns>A copy of these options whose <see cref="Transaction"/> is <paramref name="transaction"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="transaction"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><see cref="TimeoutSeconds"/> is negative.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="transaction"/> is no longer associated with a connection.</exception>
    /// <exception cref="ArgumentException">These options are already bound to another transaction.</exception>
    public CommandOptions BindTransaction(DbTransaction transaction)
    {
        if (transaction is null)
            throw new ArgumentNullException(nameof(transaction));
        var connection = transaction.Connection
                         ?? throw new InvalidOperationException(
                             "The transaction is no longer associated with a connection.");
        if (Transaction is not null && !ReferenceEquals(Transaction, transaction))
        {
            throw new ArgumentException(
                "The command options are already bound to another transaction.",
                nameof(transaction));
        }

        var boundOptions = this with { Transaction = transaction };
        boundOptions.ValidateFor(connection);
        return boundOptions;
    }

    /// <summary>
    /// Validates these options for execution against a specific connection.
    /// </summary>
    /// <param name="connection">The connection that will execute the command.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><see cref="TimeoutSeconds"/> is negative.</exception>
    /// <exception cref="InvalidOperationException"><see cref="Transaction"/> is no longer associated with a connection.</exception>
    /// <exception cref="ArgumentException"><see cref="Transaction"/> belongs to another connection.</exception>
    public void ValidateFor(DbConnection connection)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));
        if (TimeoutSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(TimeoutSeconds), TimeoutSeconds, "The command timeout cannot be negative.");
        if (Transaction is null)
            return;

        var transactionConnection = Transaction.Connection
                                    ?? throw new InvalidOperationException(
                                        "The transaction is no longer associated with a connection.");
        if (!ReferenceEquals(transactionConnection, connection))
        {
            throw new ArgumentException(
                "The transaction belongs to a different connection.",
                nameof(Transaction));
        }
    }
}
