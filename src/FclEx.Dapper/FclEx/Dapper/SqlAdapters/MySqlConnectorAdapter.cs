namespace FclEx.Dapper.SqlAdapters;

/// <summary>
/// Adapter for MySqlConnector
/// </summary>
public class MySqlConnectorAdapter : SqlAdapterBase
{
    private const int MaxParametersPerCommand = 65535;

    protected override QuotationMarks QuotationMarks { get; } = new('`');

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
        var sql = columnListSql is null && valueRowsSql is null
            ? $"INSERT INTO {quotedTableName} () VALUES ()"
            : base.BuildInsertCommandText(quotedTableName, columnListSql, valueRowsSql, null);
        return quotedGeneratedKeyColumn is null
            ? sql
            : $"{sql};{Environment.NewLine}SELECT LAST_INSERT_ID()";
    }

    protected override DbParameterCreator BuildParameterCreator()
    {
        return BuildParameterCreator("MySqlConnector.MySqlParameter, MySqlConnector", "MySqlDbType");
    }
}
