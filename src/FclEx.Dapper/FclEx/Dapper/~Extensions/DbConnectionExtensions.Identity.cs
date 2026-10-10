namespace FclEx.Dapper;

partial class DbConnectionExtensions
{
    /// <summary>Synchronizes a table's identity generator with its existing key values.</summary>
    /// <param name="connection">The caller-owned connection; its original open or closed state is preserved.</param>
    /// <param name="tableName">One unquoted table-name component.</param>
    /// <param name="columnName">One unquoted identity-column name.</param>
    /// <param name="schema">An optional schema or Oracle owner, following the adapter's CRUD namespace rules.</param>
    /// <param name="commandOptions">Optional timeout, transaction, and SQL adapter.</param>
    /// <param name="cancellationToken">Cancels opening, metadata reads, and synchronization commands.</param>
    /// <returns>True when an identity generator was synchronized; false when the table or generator is absent.</returns>
    /// <remarks>
    /// Run during maintenance without concurrent inserts, deletes, or sequence use. This is not an atomic
    /// coordination mechanism. PostgreSQL and SQL Server support ascending generators only and retain their
    /// native increment and starting value. PostgreSQL resets an empty table to its sequence start; SQL Server
    /// resets a previously used empty identity to its seed. MySQL requests MAX(key)+1, but the engine may retain
    /// a higher counter and session increment/offset settings still apply. SQLite resets AUTOINCREMENT to
    /// MAX(key), or zero when empty; ordinary ROWID tables need no synchronization and return false.
    /// Oracle 12.2+ uses START WITH LIMIT VALUE, preserving the existing identity generation mode and options;
    /// its next value follows the database's native limit calculation, rather than a portable MAX(key)+1 rule.
    /// Oracle and MySQL issue DDL with implicit commits. PostgreSQL sequence changes are not rolled back.
    /// Trigger-managed and unrelated standalone sequences are not supported. Values may be reused after reset.
    /// Permissions required by native reseeding commands and database access errors propagate to the caller.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The connection, table name, or column name is null.</exception>
    /// <exception cref="ArgumentException">An identifier is empty or whitespace, or the transaction belongs to another connection.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The command timeout is negative.</exception>
    /// <exception cref="NotSupportedException">The adapter or identity configuration is unsupported.</exception>
    /// <exception cref="InvalidOperationException">The supplied transaction is no longer associated with a connection.</exception>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    public static async Task<bool> ReseedIdentityAsync(
        this DbConnection connection, string tableName, string columnName, string? schema = null,
        CommandOptions commandOptions = default, CancellationToken cancellationToken = default)
    {
        commandOptions.ValidateFor(connection);
        ValidateIdentityIdentifier(tableName, nameof(tableName));
        ValidateIdentityIdentifier(columnName, nameof(columnName));
        if (schema is not null)
            ValidateIdentityIdentifier(schema, nameof(schema));
        cancellationToken.ThrowIfCancellationRequested();
        if (!await connection.TableExistsAsync(tableName, schema, commandOptions, cancellationToken))
            return false;

        return await connection.ExecuteAsync(commandOptions,
            _ => new SqlInfo("", Array.Empty<DbParameter>()),
            (adapter, command) => adapter.ReseedIdentityAsync(
                command, tableName, columnName, schema, cancellationToken), cancellationToken);
    }

    /// <summary>Synchronizes the identity generator for an entity's single mapped database-generated integer key.</summary>
    /// <typeparam name="T">The entity supplying the table, schema, and generated-key column mapping.</typeparam>
    /// <param name="connection">The caller-owned connection; its initial state is preserved.</param>
    /// <param name="schema">An optional schema overriding the mapped schema; null retains the mapping.</param>
    /// <param name="commandOptions">Optional timeout, transaction, SQL adapter, and mapping source.</param>
    /// <param name="cancellationToken">Cancels opening and execution.</param>
    /// <returns>True when synchronized; false when the physical table or identity generator is absent.</returns>
    /// <remarks>See the table-name overload for provider behavior, concurrency requirements, and transaction side effects.</remarks>
    /// <exception cref="DataException">The mapping does not have exactly one database-generated integer key.</exception>
    /// <exception cref="ArgumentNullException">The connection is null.</exception>
    /// <exception cref="ArgumentException">A supplied schema is empty or whitespace, or a transaction belongs to another connection.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The timeout is negative.</exception>
    /// <exception cref="NotSupportedException">The adapter or identity configuration is unsupported.</exception>
    /// <exception cref="InvalidOperationException">The supplied transaction has no connection.</exception>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    public static Task<bool> ReseedIdentityAsync<T>(
        this DbConnection connection, string? schema = null,
        CommandOptions commandOptions = default, CancellationToken cancellationToken = default)
    {
        commandOptions.ValidateFor(connection);
        var mapping = DapperHelper.GetEntityMapping(typeof(T), commandOptions.EntityMappingSource);
        if (mapping.GeneratedKeys.Count != 1 || !IsIntegerIdentity(mapping.GeneratedKeys[0].Property.PropertyType))
            throw new DataException("Identity synchronization requires exactly one database-generated integer key.");
        return connection.ReseedIdentityAsync(mapping.TableName, mapping.GeneratedKeys[0].ColumnName,
            schema ?? mapping.Schema, commandOptions, cancellationToken);
    }

    private static bool IsIntegerIdentity(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort)
            || type == typeof(int) || type == typeof(uint) || type == typeof(long) || type == typeof(ulong);
    }

    private static void ValidateIdentityIdentifier(string value, string parameterName)
    {
        if (value is null)
            throw new ArgumentNullException(parameterName);
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("An identifier cannot be empty or whitespace.", parameterName);
    }
}
