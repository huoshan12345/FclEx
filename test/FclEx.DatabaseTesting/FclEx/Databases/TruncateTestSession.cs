namespace FclEx.Databases;

/// <summary>Owns one connection and the isolated tables used by a truncate test.</summary>
/// <remarks>Temporary tables stay on this open connection. Oracle tables are leased from a fixture-owned pool.</remarks>
public sealed class TruncateTestSession : IDisposable, IEntityMappingSource
{
    private readonly Action? _release;
    private bool _disposed;

    public DbConnection Connection { get; }
    public DbDriver Driver { get; }
    public string? Schema { get; }
    public string TablePrefix { get; }
    public CommandOptions CommandOptions => new() { EntityMappingSource = this };

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
        var properties = original.Properties.Select(property => property.Property.Name == "Id"
            && (entityType == typeof(EntityHasStates) || entityType == typeof(EntityWithNavigation)
                || entityType == typeof(EntityWithIdAndIndex))
            ? new PropertyMapping(property.Property, property.ColumnName, true, DatabaseValueGeneration.OnInsert)
            : property);
        return new(entityType, GetTableName(entityType), properties, Schema);
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
            Connection.Dispose();
        }
        finally
        {
            _release?.Invoke();
        }
    }
}
