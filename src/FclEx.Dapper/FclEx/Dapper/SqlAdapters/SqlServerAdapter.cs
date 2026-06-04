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

    public override async ValueTask<IAsyncDisposable> EnableIdentityInsertAsync<T>(string? schema, IDbCommand cmd)
    {
        Check.NotNull(cmd.Connection);
        var tableName = DapperHelper.GetTableNameWithSchema(this, schema, typeof(T));
        await cmd.Connection.ExecuteAsync($"SET IDENTITY_INSERT {tableName} ON");
        return AsyncDisposable.Create(() => cmd.Connection.ExecuteAsync($"SET IDENTITY_INSERT {tableName} OFF"));
    }
}