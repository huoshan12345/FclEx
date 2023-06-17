namespace FclEx.Dapper.SqlAdapters;

public class SqlServerAdapter : ISqlAdapter
{
    public static readonly SqlServerAdapter Instance = new();

    public string GetQuotedTableName(string name) => $"[{name}]";
    public string GetQuotedColumnName(string name) => $"[{name}]";

    public DbParameter CreateParameter(string name, object? value, string? type = null)
    {
        throw new NotImplementedException();
    }

    public string SelectIdentitySql { get; } = "SELECT SCOPE_IDENTITY()"; // NOTICE: SCOPE_IDENTITY() return a decimal instead of an integer?

    public async Task<IAsyncDisposable> EnableIdentityInsertAsync<T>(string schema, IDbCommand cmd)
    {
        var tableName = DapperHelper.GetTableNameWithSchema(this, schema, typeof(T));
        await cmd.Connection.ExecuteAsync($"SET IDENTITY_INSERT {tableName} ON");
        return AsyncDisposable.Create(() => cmd.Connection.ExecuteAsync($"SET IDENTITY_INSERT {tableName} OFF"));
    }
}