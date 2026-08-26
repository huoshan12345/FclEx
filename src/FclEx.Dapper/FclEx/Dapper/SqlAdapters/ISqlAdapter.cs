namespace FclEx.Dapper.SqlAdapters;

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
    ValueTask<IAsyncDisposable> BeginExplicitIdentityInsertAsync(string quotedTableName, DbCommand command);
}
