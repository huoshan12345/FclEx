using System.Globalization;

namespace FclEx.Dapper;

partial class DbConnectionExtensions
{
    /// <summary>Checks whether a base table exists in the connection's database and requested namespace.</summary>
    /// <param name="connection">The caller-owned connection. A connection opened by this operation is closed before return.</param>
    /// <param name="tableName">One unquoted table-name component. Dots and quote characters are treated as literal name characters.</param>
    /// <param name="schema">An unquoted schema or Oracle owner; null uses native default-namespace resolution.</param>
    /// <param name="commandOptions">Optional transaction, timeout, and SQL adapter settings.</param>
    /// <param name="cancellationToken">Cancels connection opening and metadata-query execution.</param>
    /// <returns>True when the provider's catalog reports a matching table; false when it does not.</returns>
    /// <remarks>
    /// Views and synonyms are excluded. Catalog visibility depends on the current login's permissions.
    /// MySqlConnector treats schema as a database name. MySql.Data checks the selected database and ignores
    /// schema arguments, as in its CRUD adapter.
    /// SQLite checks main and temp and ignores schema arguments; attached databases are not searched.
    /// SQL Server temporary tables are not supported. Database access errors propagate instead of returning false.
    /// The result is a snapshot and does not prevent concurrent creation or deletion of the table.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The connection or table name is null.</exception>
    /// <exception cref="ArgumentException">The table name or a supplied schema is empty or whitespace.</exception>
    /// <exception cref="NotSupportedException">The SQL adapter does not support table metadata queries.</exception>
    /// <exception cref="OperationCanceledException">The operation is cancelled.</exception>
    public static Task<bool> TableExistsAsync(
        this DbConnection connection,
        string tableName,
        string? schema = null,
        CommandOptions commandOptions = default,
        CancellationToken cancellationToken = default)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));
        if (tableName is null)
            throw new ArgumentNullException(nameof(tableName));
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("A table name cannot be empty or whitespace.", nameof(tableName));
        if (schema is not null && string.IsNullOrWhiteSpace(schema))
            throw new ArgumentException("A schema name cannot be empty or whitespace.", nameof(schema));
        cancellationToken.ThrowIfCancellationRequested();

        return connection.ExecuteAsync(commandOptions, adapter =>
        {
            var parameters = new List<DbParameter> { adapter.CreateParameter("tableName", tableName) };
            string? schemaParameter = null;
            if (adapter.SupportsSchemas)
            {
                schemaParameter = adapter.GetParameterPlaceholder("schema");
                parameters.Add(adapter.CreateParameter("schema", schema));
            }
            return new SqlInfo(
                adapter.BuildTableExistsCommandText(adapter.GetParameterPlaceholder("tableName"), schemaParameter),
                parameters);
        }, async command =>
        {
            var count = await command.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt64(count, CultureInfo.InvariantCulture) > 0;
        }, cancellationToken);
    }

    /// <summary>Checks whether the table mapped to an entity type exists.</summary>
    /// <typeparam name="T">The entity type whose mapping supplies the table name and optional schema.</typeparam>
    /// <param name="connection">The caller-owned connection; its original open or closed state is preserved.</param>
    /// <param name="schema">An optional schema overriding the entity mapping. Null retains the mapped schema.</param>
    /// <param name="commandOptions">Optional transaction, timeout, SQL adapter, and entity mapping source.</param>
    /// <param name="cancellationToken">Cancels connection opening and metadata-query execution.</param>
    /// <returns>Whether the mapped base table is visible in the provider's catalog.</returns>
    /// <remarks>Uses the same provider namespace and visibility rules as the table-name overload.</remarks>
    /// <exception cref="ArgumentNullException">The connection is null.</exception>
    /// <exception cref="ArgumentException">A supplied schema is empty or whitespace.</exception>
    /// <exception cref="NotSupportedException">The SQL adapter does not support table metadata queries.</exception>
    /// <exception cref="OperationCanceledException">The operation is cancelled.</exception>
    public static Task<bool> TableExistsAsync<T>(
        this DbConnection connection,
        string? schema = null,
        CommandOptions commandOptions = default,
        CancellationToken cancellationToken = default)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));
        var mapping = DapperHelper.GetEntityMapping(typeof(T), commandOptions.EntityMappingSource);
        return connection.TableExistsAsync(mapping.TableName, schema ?? mapping.Schema, commandOptions, cancellationToken);
    }
}
