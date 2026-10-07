namespace FclEx.Dapper;

partial class DbConnectionExtensions
{
    /// <summary>Truncates an entire table using the database's native defaults or SQLite's DELETE fallback.</summary>
    /// <param name="connection">The caller-owned connection; its initial open or closed state is preserved.</param>
    /// <param name="tableName">One unquoted table-name component. Dots and quotes are literal name characters.</param>
    /// <param name="schema">An optional unquoted schema or Oracle owner, subject to the adapter's schema support.</param>
    /// <param name="commandOptions">Optional timeout, local transaction, and SQL adapter.</param>
    /// <param name="cancellationToken">Cancels connection opening and command execution.</param>
    /// <returns>A task that completes after truncation. No portable affected-row count is exposed.</returns>
    /// <remarks>
    /// SQL Server and MySQL reset identities; PostgreSQL and Oracle preserve them. SQLite deletes all rows
    /// and resets AUTOINCREMENT if present, atomically in the supplied transaction or a locally owned transaction.
    /// SQLite supports main and temp tables; attached databases are excluded. SQLite DELETE fires delete triggers
    /// and honors configured foreign-key actions. Native truncation can be restricted by foreign keys.
    /// MySQL and Oracle TRUNCATE cause implicit commits. Other rollback behavior depends on the database.
    /// Schemas follow the same rules as CRUD: MySql.Data and SQLite ignore them, MySqlConnector uses a database name.
    /// All rows are removed; entity state and application query filters are not consulted.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The connection or table name is null.</exception>
    /// <exception cref="ArgumentException">The table name or supplied schema is empty or whitespace, or the transaction belongs to another connection.</exception>
    /// <exception cref="NotSupportedException">The adapter does not support truncation.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The command timeout is negative.</exception>
    /// <exception cref="InvalidOperationException">The supplied transaction is no longer associated with a connection.</exception>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    public static Task TruncateAsync(
        this DbConnection connection, string tableName, string? schema = null,
        CommandOptions commandOptions = default, CancellationToken cancellationToken = default)
        => TruncateCoreAsync(connection, tableName, schema, null, false, commandOptions, cancellationToken);

    /// <summary>Truncates an entire table with explicit identity and cascade behavior.</summary>
    /// <param name="connection">The caller-owned connection; its initial state is preserved.</param>
    /// <param name="tableName">One unquoted table-name component.</param>
    /// <param name="restartIdentity">Whether to reset identities. SQL Server and MySQL require true; Oracle requires false.</param>
    /// <param name="cascade">Whether PostgreSQL or Oracle should truncate referencing tables. Oracle requires ON DELETE CASCADE constraints.</param>
    /// <param name="schema">An optional unquoted schema or Oracle owner.</param>
    /// <param name="commandOptions">Optional timeout, local transaction, and SQL adapter.</param>
    /// <param name="cancellationToken">Cancels connection opening and command execution.</param>
    /// <returns>A task that completes after truncation.</returns>
    /// <remarks>
    /// PostgreSQL supports all combinations. SQL Server and MySQL require true/false. Oracle supports
    /// false/false and false/true (CASCADE requires Oracle 12c or later). SQLite requires cascade=false;
    /// restartIdentity controls AUTOINCREMENT, while ordinary ROWID follows SQLite allocation rules.
    /// Unsupported options are rejected before opening the connection. CASCADE can affect other tables.
    /// See <see cref="TruncateAsync(DbConnection, string, string, CommandOptions, CancellationToken)"/>
    /// for namespace, transaction, and database side effects.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The connection or table name is null.</exception>
    /// <exception cref="ArgumentException">An identifier is empty or whitespace, or the transaction belongs to another connection.</exception>
    /// <exception cref="NotSupportedException">The adapter or requested options are unsupported.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The command timeout is negative.</exception>
    /// <exception cref="InvalidOperationException">The supplied transaction is no longer associated with a connection.</exception>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    public static Task TruncateAsync(
        this DbConnection connection, string tableName, bool restartIdentity, bool cascade,
        string? schema = null, CommandOptions commandOptions = default, CancellationToken cancellationToken = default)
        => TruncateCoreAsync(connection, tableName, schema, restartIdentity, cascade, commandOptions, cancellationToken);

    /// <summary>Truncates the entire table mapped to an entity type using native identity defaults.</summary>
    /// <typeparam name="T">The entity type whose mapping supplies the table and optional schema; no key is required.</typeparam>
    /// <param name="connection">The caller-owned connection; its initial state is preserved.</param>
    /// <param name="schema">An optional schema overriding the entity mapping; null retains its schema.</param>
    /// <param name="commandOptions">Optional timeout, transaction, SQL adapter, and entity mapping source.</param>
    /// <param name="cancellationToken">Cancels opening and execution.</param>
    /// <returns>A task that completes after truncation.</returns>
    /// <remarks>
    /// The mapping identifies a physical table; the library cannot validate inheritance or table-sharing rules.
    /// Every row in that table is removed. See the table-name overload for identity and transaction behavior.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The connection is null.</exception>
    /// <exception cref="ArgumentException">A supplied schema is empty or whitespace, or the transaction belongs to another connection.</exception>
    /// <exception cref="NotSupportedException">The adapter does not support truncation.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The command timeout is negative.</exception>
    /// <exception cref="InvalidOperationException">The supplied transaction is no longer associated with a connection.</exception>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    public static Task TruncateAsync<T>(
        this DbConnection connection, string? schema = null,
        CommandOptions commandOptions = default, CancellationToken cancellationToken = default)
    {
        commandOptions.ValidateFor(connection);
        var mapping = DapperHelper.GetEntityMapping(typeof(T), commandOptions.EntityMappingSource);
        return connection.TruncateAsync(mapping.TableName, schema ?? mapping.Schema, commandOptions, cancellationToken);
    }

    /// <summary>Truncates an entity's entire mapped table with explicit identity and cascade behavior.</summary>
    /// <typeparam name="T">The entity type supplying the table mapping; no key is required.</typeparam>
    /// <param name="connection">The caller-owned connection; its initial state is preserved.</param>
    /// <param name="restartIdentity">Whether to reset identities, subject to database support.</param>
    /// <param name="cascade">Whether to truncate referencing tables, subject to database support.</param>
    /// <param name="schema">An optional schema overriding the entity mapping.</param>
    /// <param name="commandOptions">Optional timeout, transaction, SQL adapter, and mapping source.</param>
    /// <param name="cancellationToken">Cancels opening and execution.</param>
    /// <returns>A task that completes after truncation.</returns>
    /// <remarks>Uses the option and side-effect rules of the explicit table-name overload; all mapped table rows are removed.</remarks>
    /// <exception cref="ArgumentNullException">The connection is null.</exception>
    /// <exception cref="ArgumentException">A supplied schema is empty or whitespace, or the transaction belongs to another connection.</exception>
    /// <exception cref="NotSupportedException">The adapter or requested options are unsupported.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The command timeout is negative.</exception>
    /// <exception cref="InvalidOperationException">The supplied transaction is no longer associated with a connection.</exception>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    public static Task TruncateAsync<T>(
        this DbConnection connection, bool restartIdentity, bool cascade, string? schema = null,
        CommandOptions commandOptions = default, CancellationToken cancellationToken = default)
    {
        commandOptions.ValidateFor(connection);
        var mapping = DapperHelper.GetEntityMapping(typeof(T), commandOptions.EntityMappingSource);
        return connection.TruncateAsync(mapping.TableName, restartIdentity, cascade,
            schema ?? mapping.Schema, commandOptions, cancellationToken);
    }

    private static Task TruncateCoreAsync(
        DbConnection connection, string tableName, string? schema, bool? restartIdentity, bool cascade,
        CommandOptions commandOptions, CancellationToken cancellationToken)
    {
        commandOptions.ValidateFor(connection);
        if (tableName is null)
            throw new ArgumentNullException(nameof(tableName));
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("A table name cannot be empty or whitespace.", nameof(tableName));
        if (schema is not null && string.IsNullOrWhiteSpace(schema))
            throw new ArgumentException("A schema name cannot be empty or whitespace.", nameof(schema));
        cancellationToken.ThrowIfCancellationRequested();
        return connection.ExecuteAsync(commandOptions, adapter =>
        {
            var table = adapter.GetQuotedTableName(tableName);
            if (adapter.SupportsSchemas && schema is not null)
                table = adapter.GetQuotedTableName(schema) + "." + table;
            return new SqlInfo(adapter.BuildTruncateCommandText(table, restartIdentity, cascade), Array.Empty<DbParameter>());
        }, (adapter, command) => adapter.ExecuteTruncateAsync(command, tableName, restartIdentity, cancellationToken), cancellationToken);
    }
}
