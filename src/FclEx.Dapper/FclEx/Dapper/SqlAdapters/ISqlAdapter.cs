namespace FclEx.Dapper.SqlAdapters;

/// <summary>
/// Defines provider-specific SQL generation and parameter behavior used by FclEx.Dapper CRUD operations.
/// </summary>
/// <remarks>
/// An adapter registered with <see cref="DapperHelper.RegisterSqlAdapter(Type, ISqlAdapter)"/> must keep all
/// SQL-affecting behavior stable while registered because generated SQL is cached by adapter instance.
/// </remarks>
public interface ISqlAdapter
{
    /// <summary>
    /// Gets whether table names may be qualified by a schema.
    /// </summary>
    /// <remarks>
    /// When this is <see langword="false"/>, FclEx.Dapper ignores schemas supplied by mappings and method arguments.
    /// </remarks>
    bool SupportsSchemas { get; }

    /// <summary>
    /// Quotes and escapes one unqualified table-name component for this SQL dialect.
    /// </summary>
    /// <param name="name">The unquoted table or schema name from trusted application configuration.</param>
    /// <returns>The delimited identifier, with embedded terminating delimiters escaped.</returns>
    string GetQuotedTableName(string name);

    /// <summary>
    /// Quotes and escapes one column-name component for this SQL dialect.
    /// </summary>
    /// <param name="name">The unquoted column name from trusted application configuration.</param>
    /// <returns>The delimited identifier, with embedded terminating delimiters escaped.</returns>
    string GetQuotedColumnName(string name);

    /// <summary>
    /// Gets the maximum number of rows that one parameterized multi-row INSERT command can contain.
    /// </summary>
    /// <param name="parameterCountPerRow">The positive number of parameters required for one row.</param>
    /// <returns>A positive row count that observes the provider's command parameter and row limits.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="parameterCountPerRow"/> is not positive.</exception>
    /// <exception cref="NotSupportedException">One row already exceeds the provider's parameter limit.</exception>
    int GetMaxInsertBatchSize(int parameterCountPerRow);

    /// <summary>
    /// Builds a complete single-row or multi-row INSERT command from pre-quoted and pre-rendered SQL fragments.
    /// </summary>
    /// <param name="quotedTableName">The fully qualified and quoted target table name.</param>
    /// <param name="columnListSql">
    /// The comma-separated quoted column list, or <see langword="null"/> for a row containing only database defaults.
    /// </param>
    /// <param name="valueRowsSql">
    /// The comma-separated parameterized value rows, or <see langword="null"/> for a row containing only database defaults.
    /// </param>
    /// <param name="quotedGeneratedKeyColumn">
    /// The quoted generated-key column to return, or <see langword="null"/> when no key result is requested.
    /// </param>
    /// <returns>The complete provider-specific INSERT command text.</returns>
    /// <exception cref="ArgumentException">
    /// Exactly one of <paramref name="columnListSql"/> and <paramref name="valueRowsSql"/> is null.
    /// </exception>
    /// <exception cref="NotSupportedException">The adapter cannot return a requested generated key.</exception>
    string BuildInsertCommandText(
        string quotedTableName,
        string? columnListSql,
        string? valueRowsSql,
        string? quotedGeneratedKeyColumn);

    /// <summary>
    /// Creates a provider parameter for a generated CRUD command.
    /// </summary>
    /// <param name="name">The parameter name used by the generated command text.</param>
    /// <param name="value">The CLR value; null is represented as <see cref="DBNull.Value"/>.</param>
    /// <param name="storeTypeName">
    /// An optional provider-specific store type name. An adapter may leave the provider to infer the type when the
    /// name is absent or unrecognized.
    /// </param>
    /// <returns>A provider-specific parameter containing the supplied name and value.</returns>
    DbParameter CreateParameter(string name, object? value, string? storeTypeName = null);

    /// <summary>
    /// Starts the provider-specific scope required to insert explicit generated-key values.
    /// </summary>
    /// <param name="quotedTableName">The fully quoted table name.</param>
    /// <param name="command">A command carrying the target connection and transaction.</param>
    /// <param name="cancellationToken">The token used to cancel scope setup.</param>
    /// <returns>A scope whose disposal restores the provider's normal identity-insert behavior.</returns>
    ValueTask<IAsyncDisposable> BeginExplicitIdentityInsertAsync(
        string quotedTableName,
        DbCommand command,
        CancellationToken cancellationToken = default);
}
