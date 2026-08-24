namespace FclEx.Dapper.SqlAdapters;

/// <summary>
/// Adapter for Microsoft.Data.SqlClient
/// </summary>
public class SqlServerAdapter : SqlAdapterBase<SqlServerAdapter>
{
    public override string SelectIdentitySql { get; } = "SELECT SCOPE_IDENTITY()"; // NOTICE: SCOPE_IDENTITY() return a decimal instead of an integer?

    protected override QuotationMarks QuotationMarks { get; } = new('[', ']');

    protected override DbParameterCreator BuildParameterCreator()
    {
        return BuildParameterCreator("Microsoft.Data.SqlClient.SqlParameter, Microsoft.Data.SqlClient", "SqlDbType");
    }

    public override async ValueTask<IAsyncDisposable> EnableIdentityInsertAsync(string quotedTableName, IDbCommand command)
    {
        Check.NotNull(command.Connection);
        await command.Connection.ExecuteAsync($"SET IDENTITY_INSERT {quotedTableName} ON");
        return AsyncDisposable.Create(() => command.Connection.ExecuteAsync($"SET IDENTITY_INSERT {quotedTableName} OFF"));
    }
}
