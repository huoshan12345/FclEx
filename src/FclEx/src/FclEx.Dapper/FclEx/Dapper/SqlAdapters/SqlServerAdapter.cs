namespace FclEx.Dapper.SqlAdapters;

public class SqlServerAdapter : AbstractSqlAdapter<SqlServerAdapter>
{
    public override string SelectIdentitySql { get; } = "SELECT SCOPE_IDENTITY()"; // NOTICE: SCOPE_IDENTITY() return a decimal instead of an integer?

    protected override QuotationMarks QuotationMarks { get; } = new('[', ']');

    public override DbParameter CreateParameter(string name, object? value, string? type = null)
    {
        throw new NotImplementedException();
    }

    public override async Task<IAsyncDisposable> EnableIdentityInsertAsync<T>(string schema, IDbCommand cmd)
    {
        var tableName = DapperHelper.GetTableNameWithSchema(this, schema, typeof(T));
        await cmd.Connection.ExecuteAsync($"SET IDENTITY_INSERT {tableName} ON");
        return AsyncDisposable.Create(() => cmd.Connection.ExecuteAsync($"SET IDENTITY_INSERT {tableName} OFF"));
    }
}