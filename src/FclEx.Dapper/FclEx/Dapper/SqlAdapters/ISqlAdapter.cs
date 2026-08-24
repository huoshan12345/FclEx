namespace FclEx.Dapper.SqlAdapters;

public interface ISqlAdapter
{
    bool SupportSchema { get; }
    string SelectIdentitySql { get; }
    string GetQuotedTableName(string name);
    string GetQuotedColumnName(string name);
    /// <summary>
    /// Creates a provider parameter and applies a recognized provider-specific store type when possible.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="value">The parameter value.</param>
    /// <param name="storeTypeName">
    /// An optional database store type name. The provider infers the parameter type when the adapter does not recognize it.
    /// </param>
    /// <returns>The provider parameter.</returns>
    DbParameter CreateParameter(string name, object? value, string? storeTypeName = null);
    /// <summary>
    /// Enables explicit identity-value insertion for a table when the provider requires it.
    /// </summary>
    /// <param name="quotedTableName">The already quoted and schema-qualified table name.</param>
    /// <param name="command">The insert command whose connection owns the identity-insert scope.</param>
    /// <returns>A scope that disables identity insertion when disposed, or an empty scope when unsupported or unnecessary.</returns>
    ValueTask<IAsyncDisposable> EnableIdentityInsertAsync(string quotedTableName, IDbCommand command);
}
