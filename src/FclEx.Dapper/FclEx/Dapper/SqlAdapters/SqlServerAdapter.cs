namespace FclEx.Dapper.SqlAdapters;

/// <summary>
/// Provides SQL Server quoting, parameter construction, batch limits, generated-key retrieval, and identity-insert handling for Microsoft.Data.SqlClient.
/// </summary>
public class SqlServerAdapter : SqlAdapterBase
{
    /// <inheritdoc />
    public override async Task<bool> ReseedIdentityAsync(DbCommand command, string tableName, string columnName,
        string? schema, CancellationToken cancellationToken = default)
    {
        var table = PrepareIdentityCommand(command, tableName, columnName, schema);
        command.Parameters.Clear();
        command.Parameters.Add(CreateParameter("table", table));
        command.Parameters.Add(CreateParameter("column", columnName));
        const string predicate = "FROM sys.identity_columns WHERE object_id = OBJECT_ID(@table, 'U') AND name = @column";
        var seedValue = await ReadIdentityScalarAsync(command, "SELECT seed_value " + predicate, cancellationToken);
        if (seedValue is null or DBNull)
            return false;
        var increment = Convert.ToDecimal(await ReadIdentityScalarAsync(command, "SELECT increment_value " + predicate, cancellationToken));
        if (increment <= 0)
            throw new NotSupportedException("Identity synchronization requires an ascending SQL Server identity.");
        var maximum = await ReadIdentityScalarAsync(command,
            $"SELECT MAX({GetQuotedColumnName(columnName)}) FROM {table}", cancellationToken);
        if (maximum is null or DBNull)
        {
            var last = await ReadIdentityScalarAsync(command, "SELECT last_value " + predicate, cancellationToken);
            // A never-used or truncated identity already starts at its seed; DBCC would change first-insert semantics.
            if (last is null or DBNull)
                return true;
        }
        var reseed = maximum is null or DBNull
            ? checked(Convert.ToDecimal(seedValue) - increment) : Convert.ToDecimal(maximum);
        command.Parameters.Clear();
        command.CommandText = $"DBCC CHECKIDENT (N'{table.Replace("'", "''")}', RESEED, {reseed.ToString(System.Globalization.CultureInfo.InvariantCulture)}) WITH NO_INFOMSGS";
        await command.ExecuteNonQueryAsync(cancellationToken);
        return true;
    }

    /// <inheritdoc />
    public override string BuildTruncateCommandText(string quotedTableName, bool? restartIdentity, bool cascade)
    {
        if (restartIdentity == false || cascade)
            throw new NotSupportedException("SQL Server TRUNCATE requires restartIdentity=true and cascade=false.");
        return $"TRUNCATE TABLE {quotedTableName};";
    }

    /// <inheritdoc />
    /// <remarks>Uses SQL Server's default-schema/dbo name resolution and metadata visibility rules. Temporary tables are excluded.</remarks>
    public override string BuildTableExistsCommandText(string tableNameParameter, string? schemaParameter = null)
    {
        var table = $"QUOTENAME({tableNameParameter})";
        if (schemaParameter is not null)
            table = $"CASE WHEN {schemaParameter} IS NULL THEN {table} ELSE QUOTENAME({schemaParameter}) + '.' + {table} END";
        return $"SELECT COUNT(*) FROM sys.tables WHERE object_id = OBJECT_ID({table}, 'U')";
    }

    private const int MaxParametersPerCommand = 2100;
    private const int MaxRowsPerValuesClause = 1000;

    /// <inheritdoc />
    protected override QuotationMarks QuotationMarks { get; } = new('[', ']');

    /// <inheritdoc />
    /// <remarks>Applies SQL Server's 2,100-parameter command limit and 1,000-row VALUES limit.</remarks>
    public override int GetMaxInsertBatchSize(int parameterCountPerRow)
    {
        return Math.Min(
            MaxRowsPerValuesClause,
            CalculateMaxInsertBatchSize(parameterCountPerRow, MaxParametersPerCommand));
    }

    /// <inheritdoc />
    /// <remarks>Uses <c>OUTPUT INSERTED</c> when a generated-key column is requested.</remarks>
    public override string BuildInsertCommandText(
        string quotedTableName,
        string? columnListSql,
        string? valueRowsSql,
        string? quotedGeneratedKeyColumn)
    {
        if ((columnListSql is null) != (valueRowsSql is null))
            throw new ArgumentException("The column list and value rows must either both be supplied or both be null.");

        var outputClause = quotedGeneratedKeyColumn is null
            ? string.Empty
            : $"{Environment.NewLine}OUTPUT INSERTED.{quotedGeneratedKeyColumn}";

        return columnListSql is null
            ? $"INSERT INTO {quotedTableName}{outputClause}{Environment.NewLine}DEFAULT VALUES"
            : $"INSERT INTO {quotedTableName} ({columnListSql}){outputClause}{Environment.NewLine}VALUES{Environment.NewLine}{valueRowsSql}";
    }

    /// <inheritdoc />
    protected override DbParameterCreator BuildParameterCreator()
    {
        return BuildParameterCreator("Microsoft.Data.SqlClient.SqlParameter, Microsoft.Data.SqlClient", "SqlDbType");
    }

    /// <inheritdoc />
    public override async ValueTask<IAsyncDisposable> BeginExplicitIdentityInsertAsync(
        string quotedTableName,
        DbCommand command,
        CancellationToken cancellationToken = default)
    {
        var connection = command.Connection ?? throw new InvalidOperationException("The command must have a connection.");
        var transaction = command.Transaction;
        await connection.ExecuteAsync(new CommandDefinition(
            $"SET IDENTITY_INSERT {quotedTableName} ON",
            transaction: transaction,
            cancellationToken: cancellationToken));

        // Disabling IDENTITY_INSERT is cleanup. It must still run after the caller's token is cancelled.
        return AsyncDisposable.Create(async () =>
        {
            await connection.ExecuteAsync(new CommandDefinition(
                $"SET IDENTITY_INSERT {quotedTableName} OFF",
                transaction: transaction,
                cancellationToken: CancellationToken.None));
        });
    }
}
