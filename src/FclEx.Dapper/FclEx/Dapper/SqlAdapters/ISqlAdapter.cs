namespace FclEx.Dapper.SqlAdapters;

public interface ISqlAdapter
{
    bool SupportSchema { get; }
    string SelectIdentitySql { get; }
    string GetQuotedTableName(string name);
    string GetQuotedColumnName(string name);
    DbParameter CreateParameter(string name, object? value, string? type = null);
    ValueTask<IAsyncDisposable> EnableIdentityInsertAsync<T>(string? schema, IDbCommand cmd);
}