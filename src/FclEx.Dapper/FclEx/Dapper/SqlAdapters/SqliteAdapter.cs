namespace FclEx.Dapper.SqlAdapters;

/// <summary>
/// Provides SQLite quoting, parameter construction, batch limits, and generated-key retrieval for Microsoft.Data.Sqlite.
/// </summary>
public class SqliteAdapter : SqlAdapterBase
{
    /// <inheritdoc />
    public override async Task<bool> SynchronizeIdentitySequenceAsync(DbCommand command, string tableName, string columnName,
        string? schema, CancellationToken cancellationToken = default)
    {
        if (command.Transaction is not null)
            return await SynchronizeSequenceCoreAsync(command, tableName, columnName, cancellationToken);
        return await command.Connection!.ExecuteInTransactionAsync(async (transaction, token) =>
        {
            command.Transaction = transaction;
            try
            {
                return await SynchronizeSequenceCoreAsync(command, tableName, columnName, token);
            }
            finally
            {
                command.Transaction = null;
            }
        }, IsolationLevel.Serializable, cancellationToken);
    }

    private async Task<bool> SynchronizeSequenceCoreAsync(DbCommand command, string tableName, string columnName,
        CancellationToken cancellationToken)
    {
        PrepareIdentityCommand(command, tableName, columnName, null);
        command.Parameters.Clear();
        command.Parameters.Add(CreateParameter("tableName", tableName));
        command.Parameters.Add(CreateParameter("columnName", columnName));
        var temp = await ReadIdentityScalarAsync(command,
            "SELECT COUNT(*) FROM temp.sqlite_master WHERE type = 'table' AND name = @tableName COLLATE NOCASE", cancellationToken);
        var database = Convert.ToInt64(temp) > 0 ? "temp" : "main";
        var definition = await ReadIdentityScalarAsync(command,
            $"SELECT sql FROM {database}.sqlite_master WHERE type = 'table' AND name = @tableName COLLATE NOCASE", cancellationToken);
        // Ignore comments, string literals, and delimited identifiers when looking for the SQLite keyword.
        var tokens = Regex.Replace(Convert.ToString(definition) ?? "",
            "--[^\\r\\n]*|/\\*[\\s\\S]*?\\*/|'(?:''|[^'])*'|\"(?:\"\"|[^\"])*\"|`(?:``|[^`])*`|\\[[^\\]]*\\]", " ");
        if (!Regex.IsMatch(tokens, @"\bAUTOINCREMENT\b", RegexOptions.IgnoreCase))
            return false;
        var key = await ReadIdentityScalarAsync(command,
            $"SELECT COUNT(*) FROM pragma_table_info(@tableName, '{database}') WHERE name = @columnName COLLATE NOCASE AND pk = 1 AND upper(type) = 'INTEGER'",
            cancellationToken);
        if (Convert.ToInt64(key) == 0)
            return false;
        // DELETE restores the empty-table default too. SQLite derives the next ROWID from MAX when no entry exists.
        command.CommandText = $"DELETE FROM {database}.sqlite_sequence WHERE name = @tableName COLLATE NOCASE";
        await command.ExecuteNonQueryAsync(cancellationToken);
        command.CommandText = $"INSERT INTO {database}.sqlite_sequence(name, seq) SELECT name, " +
            $"MAX(0, COALESCE((SELECT MAX({GetQuotedColumnName(columnName)}) FROM {database}.{GetQuotedTableName(tableName)}), 0)) " +
            $"FROM {database}.sqlite_master WHERE type = 'table' AND name = @tableName COLLATE NOCASE";
        await command.ExecuteNonQueryAsync(cancellationToken);
        return true;
    }

    /// <inheritdoc />
    public override string BuildTruncateCommandText(string quotedTableName, bool? restartIdentity, bool cascade)
    {
        if (cascade)
            throw new NotSupportedException("SQLite does not support TRUNCATE CASCADE.");
        return $"DELETE FROM {quotedTableName};";
    }

    /// <inheritdoc />
    public override async Task<int> ExecuteTruncateAsync(
        DbCommand command, string tableName, bool? restartIdentity, CancellationToken cancellationToken = default)
    {
        if (command.Transaction is not null)
            return await DeleteTableAsync(command, tableName, restartIdentity != false, cancellationToken);

        return await command.Connection!.ExecuteInTransactionAsync(async (transaction, token) =>
        {
            command.Transaction = transaction;
            try
            {
                return await DeleteTableAsync(command, tableName, restartIdentity != false, token);
            }
            finally
            {
                command.Transaction = null;
            }
        }, IsolationLevel.Serializable, cancellationToken);
    }

    private async Task<int> DeleteTableAsync(DbCommand command, string tableName, bool restartIdentity, CancellationToken cancellationToken)
    {
        // Unqualified SQLite names resolve to temp before main. Use that same namespace for
        // both deletion and sequence maintenance; attached databases are outside this API.
        command.Parameters.Add(CreateParameter("tableName", tableName));
        var parameter = GetParameterPlaceholder("tableName");
        command.CommandText = $"SELECT COUNT(*) FROM temp.sqlite_master WHERE type = 'table' AND name = {parameter} COLLATE NOCASE";
        var database = Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken)) != 0 ? "temp" : "main";
        command.CommandText = $"DELETE FROM {database}.{GetQuotedTableName(tableName)};";
        var deleted = await command.ExecuteNonQueryAsync(cancellationToken);
        if (restartIdentity)
        {
            command.CommandText = $"SELECT COUNT(*) FROM {database}.sqlite_master WHERE type = 'table' AND name = 'sqlite_sequence'";
            if (Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken)) != 0)
            {
                command.CommandText = $"DELETE FROM {database}.sqlite_sequence WHERE name = {parameter} COLLATE NOCASE;";
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }
        return deleted;
    }

    /// <inheritdoc />
    /// <remarks>Checks main and temp using SQLite's case-insensitive identifier comparison. Attached databases and schema arguments are excluded.</remarks>
    public override string BuildTableExistsCommandText(string tableNameParameter, string? schemaParameter = null)
        => $"""
            SELECT COUNT(*) FROM (
                SELECT name, type FROM main.sqlite_master
                UNION ALL
                SELECT name, type FROM temp.sqlite_master
            ) WHERE type = 'table' AND name = {tableNameParameter} COLLATE NOCASE
            """;
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
