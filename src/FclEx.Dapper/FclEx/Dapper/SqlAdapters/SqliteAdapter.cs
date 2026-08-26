namespace FclEx.Dapper.SqlAdapters;

/// <summary>
/// Adapter for Microsoft.Data.Sqlite
/// </summary>
public class SqliteAdapter : SqlAdapterBase<SqliteAdapter>
{
    private const int MaxParametersPerCommand = 999;

    public override bool SupportsSchemas { get; } = false;

    protected override QuotationMarks QuotationMarks { get; } = new('"');

    public override int GetMaxInsertBatchSize(int parameterCountPerRow)
    {
        return CalculateMaxInsertBatchSize(parameterCountPerRow, MaxParametersPerCommand);
    }

    public override string BuildInsertCommandText(
        string quotedTableName,
        string? columnListSql,
        string? valueRowsSql,
        string? quotedGeneratedKeyColumn)
    {
        var sql = base.BuildInsertCommandText(quotedTableName, columnListSql, valueRowsSql, null);
        return quotedGeneratedKeyColumn is null
            ? sql
            : $"{sql};{Environment.NewLine}SELECT last_insert_rowid()";
    }

    protected override DbParameterCreator BuildParameterCreator()
    {
        return BuildParameterCreator("Microsoft.Data.Sqlite.SqliteParameter, Microsoft.Data.Sqlite", "SqliteType");
    }
}
