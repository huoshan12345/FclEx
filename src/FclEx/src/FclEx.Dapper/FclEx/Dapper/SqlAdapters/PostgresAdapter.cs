namespace FclEx.Dapper.SqlAdapters;

public class PostgresAdapter : ISqlAdapter
{
    public static readonly PostgresAdapter Instance = new();
    
    public string SelectIdentitySql { get; } = "SELECT LASTVAL()";
    public string GetQuotedTableName(string name) => $"\"{name}\"";
    public string GetQuotedColumnName(string name) => $"\"{name}\"";

    public DbParameter CreateParameter(string name, object? value, string? type = null)
    {
        throw new NotImplementedException();
    }

    public Task<IAsyncDisposable> EnableIdentityInsertAsync<T>(string schema, IDbCommand cmd) => AsyncDisposable.EmptyTask;
}