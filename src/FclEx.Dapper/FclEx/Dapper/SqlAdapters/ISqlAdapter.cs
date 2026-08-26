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
    bool SupportsSchemas { get; }
    string GetQuotedTableName(string name);
    string GetQuotedColumnName(string name);
    int GetMaxInsertBatchSize(int parameterCountPerRow);
    string BuildInsertCommandText(
        string quotedTableName,
        string? columnListSql,
        string? valueRowsSql,
        string? quotedGeneratedKeyColumn);
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
