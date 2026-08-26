namespace FclEx.Dapper.SqlAdapters;

/// <summary>
/// Adapter for Npgsql
/// </summary>
public class NpgsqlAdapter : SqlAdapterBase<NpgsqlAdapter>
{
    private const int MaxParametersPerCommand = 65535;

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
            : $"{sql}{Environment.NewLine}RETURNING {quotedGeneratedKeyColumn}";
    }
    
    protected override DbParameterCreator BuildParameterCreator()
    {
        return BuildParameterCreator("Npgsql.NpgsqlParameter, Npgsql", "NpgsqlDbType");
    }
}
