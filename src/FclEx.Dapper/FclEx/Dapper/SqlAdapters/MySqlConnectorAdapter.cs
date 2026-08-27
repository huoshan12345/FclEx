namespace FclEx.Dapper.SqlAdapters;

/// <summary>
/// Provides MySQL/MariaDB quoting, parameter construction, batch limits, and generated-key retrieval for MySqlConnector.
/// </summary>
public class MySqlConnectorAdapter : SqlAdapterBase
{
    private const int MaxParametersPerCommand = 65535;

    /// <inheritdoc />
    protected override QuotationMarks QuotationMarks { get; } = new('`');

    /// <inheritdoc />
    /// <remarks>Applies MySQL's 65,535-parameter command limit.</remarks>
    public override int GetMaxInsertBatchSize(int parameterCountPerRow)
    {
        return CalculateMaxInsertBatchSize(parameterCountPerRow, MaxParametersPerCommand);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Uses MySQL's empty-row syntax for a default-only insert and appends <c>SELECT LAST_INSERT_ID()</c> when a
    /// generated-key column is requested.
    /// </remarks>
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

    /// <inheritdoc />
    protected override DbParameterCreator BuildParameterCreator()
    {
        return BuildParameterCreator("MySqlConnector.MySqlParameter, MySqlConnector", "MySqlDbType");
    }
}
