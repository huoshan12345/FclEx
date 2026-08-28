namespace FclEx.Dapper.SqlAdapters;

/// <summary>
/// Provides SQLite quoting, parameter construction, batch limits, and generated-key retrieval for Microsoft.Data.Sqlite.
/// </summary>
public class SqliteAdapter : SqlAdapterBase
{
    private const int MaxParametersPerCommand = 999;

    /// <inheritdoc />
    /// <remarks>SQLite table names are not qualified by schemas.</remarks>
    public override bool SupportsSchemas { get; } = false;

    /// <inheritdoc />
    protected override QuotationMarks QuotationMarks { get; } = new('"');

    /// <inheritdoc />
    /// <remarks>Applies SQLite's configured 999-parameter command limit.</remarks>
    public override int GetMaxInsertBatchSize(int parameterCountPerRow)
    {
        return CalculateMaxInsertBatchSize(parameterCountPerRow, MaxParametersPerCommand);
    }

    /// <inheritdoc />
    /// <remarks>Appends <c>SELECT last_insert_rowid()</c> when a generated-key column is requested.</remarks>
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

    /// <inheritdoc />
    protected override DbParameterCreator BuildParameterCreator()
    {
        return BuildParameterCreator("Microsoft.Data.Sqlite.SqliteParameter, Microsoft.Data.Sqlite", "SqliteType");
    }
}
