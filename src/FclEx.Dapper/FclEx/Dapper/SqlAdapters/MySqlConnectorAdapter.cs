namespace FclEx.Dapper.SqlAdapters;

/// <summary>
/// Provides MySQL/MariaDB quoting, parameter construction, batch limits, and generated-key retrieval for MySqlConnector.
/// </summary>
public class MySqlConnectorAdapter : SqlAdapterBase
{
    /// <inheritdoc />
    public override async Task<bool> ReseedIdentityAsync(DbCommand command, string tableName, string columnName,
        string? schema, CancellationToken cancellationToken = default)
    {
        var table = PrepareIdentityCommand(command, tableName, columnName, schema);
        var count = await ReadIdentityScalarAsync(command,
            "SELECT COUNT(*) FROM information_schema.columns WHERE table_schema = COALESCE(@schema, DATABASE()) " +
            "AND table_name = @tableName AND column_name = @columnName AND extra LIKE '%auto_increment%'", cancellationToken);
        if (Convert.ToInt64(count) == 0)
            return false;
        var maximum = await ReadIdentityScalarAsync(command,
            $"SELECT MAX({GetQuotedColumnName(columnName)}) FROM {table}", cancellationToken);
        var next = maximum is null or DBNull ? 1m : Math.Max(1m, checked(Convert.ToDecimal(maximum) + 1m));
        command.Parameters.Clear();
        command.CommandText = $"ALTER TABLE {table} AUTO_INCREMENT = {next.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
        await command.ExecuteNonQueryAsync(cancellationToken);
        return true;
    }

    /// <inheritdoc />
    public override string BuildTruncateCommandText(string quotedTableName, bool? restartIdentity, bool cascade)
    {
        if (restartIdentity == false || cascade)
            throw new NotSupportedException("MySQL TRUNCATE requires restartIdentity=true and cascade=false.");
        return $"TRUNCATE TABLE {quotedTableName};";
    }

    /// <inheritdoc />
    /// <remarks>Uses the schema as a database name, defaulting to the connection's selected database.</remarks>
    public override string BuildTableExistsCommandText(string tableNameParameter, string? schemaParameter = null)
    {
        var database = schemaParameter is null ? "DATABASE()" : $"COALESCE({schemaParameter}, DATABASE())";
        return $"SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = {database} AND table_name = {tableNameParameter} AND table_type = 'BASE TABLE'";
    }

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
