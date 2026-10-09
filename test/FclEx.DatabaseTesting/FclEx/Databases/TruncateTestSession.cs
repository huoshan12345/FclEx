namespace FclEx.Databases;

/// <summary>Owns a truncate test's connection and dedicated table mappings.</summary>
public sealed class TruncateTestSession : IDisposable, IEntityMappingSource
{
    private readonly Action? _release;
    private bool _disposed;

    public DbConnection Connection { get; }
    public DbDriver Driver { get; }
    public string? Schema { get; }
    public string TablePrefix { get; }
    public CommandOptions CommandOptions => new() { EntityMappingSource = this };
    internal List<string> TablesToDrop { get; } = [];

    internal TruncateTestSession(DbConnection connection, DbDriver driver, string? schema, string prefix, Action? release = null)
    {
        Connection = connection;
        Driver = driver;
        Schema = schema;
        TablePrefix = prefix;
        _release = release;
    }

    public string GetTableName(Type entityType) => TablePrefix + DapperHelper.GetEntityMapping(entityType).TableName;

    public EntityMapping GetMapping(Type entityType)
    {
        var original = DapperHelper.GetEntityMapping(entityType);
        return new(entityType, GetTableName(entityType), original.Properties, Schema);
    }

    public string GetQualifiedTableName(Type entityType)
        => DapperHelper.GetTableNameWithSchema(Connection, Schema, entityType, this);

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        try
        {
            foreach (var table in TablesToDrop.AsEnumerable().Reverse())
                Connection.Execute($"DROP TABLE {table}");
        }
        finally
        {
            try
            {
                Connection.Dispose();
            }
            finally
            {
                _release?.Invoke();
            }
        }
    }
}
