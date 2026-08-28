namespace FclEx.Dapper.SqlAdapters;

/// <summary>
/// Provides PostgreSQL quoting, parameter construction, batch limits, and generated-key retrieval for Npgsql.
/// </summary>
public class NpgsqlAdapter : SqlAdapterBase
{
    private const int MaxParametersPerCommand = 65535;

    /// <inheritdoc />
    protected override QuotationMarks QuotationMarks { get; } = new('"');

    /// <inheritdoc />
    /// <remarks>Applies PostgreSQL's 65,535-parameter command limit.</remarks>
    public override int GetMaxInsertBatchSize(int parameterCountPerRow)
    {
        return CalculateMaxInsertBatchSize(parameterCountPerRow, MaxParametersPerCommand);
    }

    /// <inheritdoc />
    /// <remarks>Appends <c>RETURNING</c> when a generated-key column is requested.</remarks>
    public override string BuildInsertCommandText(
        string quotedTableName,
        string? columnListSql,
        string? valueRowsSql,
        string? quotedGeneratedKeyColumn)
    {
        var sql = base.BuildInsertCommandText(quotedTableName, columnListSql, valueRowsSql, null);
        return quotedGeneratedKeyColumn is null
            ? sql
            : $"{sql}{Environment.NewLine}RETURNING {quotedGeneratedKeyColumn}";
    }
    
    /// <inheritdoc />
    protected override DbParameterCreator BuildParameterCreator()
    {
        return BuildParameterCreator("Npgsql.NpgsqlParameter, Npgsql", "NpgsqlDbType");
    }
}
