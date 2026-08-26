namespace FclEx.Dapper.SqlAdapters;

/// <summary>
/// Adapter for Microsoft.Data.SqlClient
/// </summary>
public class SqlServerAdapter : SqlAdapterBase<SqlServerAdapter>
{
    private const int MaxParametersPerCommand = 2100;
    private const int MaxRowsPerValuesClause = 1000;

    protected override QuotationMarks QuotationMarks { get; } = new('[', ']');

    public override int GetMaxInsertBatchSize(int parameterCountPerRow)
    {
        return Math.Min(
            MaxRowsPerValuesClause,
            CalculateMaxInsertBatchSize(parameterCountPerRow, MaxParametersPerCommand));
    }

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

    protected override DbParameterCreator BuildParameterCreator()
    {
        return BuildParameterCreator("Microsoft.Data.SqlClient.SqlParameter, Microsoft.Data.SqlClient", "SqlDbType");
    }

    public override async ValueTask<IAsyncDisposable> BeginExplicitIdentityInsertAsync(string quotedTableName, DbCommand command)
    {
        var connection = command.Connection ?? throw new InvalidOperationException("The command must have a connection.");
        var transaction = command.Transaction;
        await connection.ExecuteAsync($"SET IDENTITY_INSERT {quotedTableName} ON", transaction: transaction);
        return AsyncDisposable.Create(() => connection.ExecuteAsync($"SET IDENTITY_INSERT {quotedTableName} OFF", transaction: transaction));
    }
}
