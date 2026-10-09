namespace FclEx.Databases;

/// <summary>Owns a truncate test's connection and exclusive leases on existing ordinary tables.</summary>
public sealed class TruncateTestSession : IDisposable
{
    private readonly Action _release;
    private bool _disposed;

    public DbConnection Connection { get; }
    public DbDriver Driver { get; }
    public string? Schema { get; }
    internal TruncateTestSession(DbConnection connection, DbDriver driver, string? schema, Action release)
    {
        Connection = connection;
        Driver = driver;
        Schema = schema;
        _release = release;
    }

#pragma warning disable CA1822 // Mark members as static
    public string GetTableName(Type entityType) => DapperHelper.GetEntityMapping(entityType).TableName;
#pragma warning restore CA1822 // Mark members as static

    public string GetQualifiedTableName(Type entityType)
        => DapperHelper.GetTableNameWithSchema(Connection, Schema, entityType);

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        try
        {
            Connection.Dispose();
        }
        finally
        {
            _release();
        }
    }
}
