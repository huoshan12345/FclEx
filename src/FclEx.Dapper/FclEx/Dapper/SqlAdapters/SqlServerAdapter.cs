namespace FclEx.Dapper.SqlAdapters;

/// <summary>
/// Provides SQL Server quoting, parameter construction, batch limits, generated-key retrieval, and identity-insert handling for Microsoft.Data.SqlClient.
/// </summary>
public class SqlServerAdapter : SqlAdapterBase
{
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
