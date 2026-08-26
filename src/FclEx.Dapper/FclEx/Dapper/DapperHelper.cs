namespace FclEx.Dapper;

public static class DapperHelper
{
    private static readonly ConcurrentDictionary<string, ISqlAdapter> _adapters = new()
    {
        ["Npgsql.NpgsqlConnection"] = NpgsqlAdapter.Instance,
        ["Microsoft.Data.SqlClient.SqlConnection"] = SqlServerAdapter.Instance,
        ["Microsoft.Data.Sqlite.SqliteConnection"] = SqliteAdapter.Instance,
        ["MySql.Data.MySqlClient.MySqlConnection"] = MySqlAdapter.Instance,
        ["MySqlConnector.MySqlConnection"] = MySqlConnectorAdapter.Instance,
    };
    private static readonly ConcurrentDictionary<(ISqlAdapter Adapter, string? Schema, EntityMapping Mapping), string> _tableFullNames = new();

    /// <summary>
    /// Gets the default mapping source used when an operation does not specify one.
    /// </summary>
    public static IEntityMappingSource DefaultEntityMappingSource { get; } = DataAnnotationsEntityMappingSource.Instance;

    /// <summary>
    /// Gets an entity mapping from the supplied source or from <see cref="DefaultEntityMappingSource"/>.
    /// </summary>
    /// <param name="entityType">The CLR entity type.</param>
    /// <param name="mappingSource">An optional mapping source.</param>
    /// <returns>The stable mapping for <paramref name="entityType"/>.</returns>
    public static EntityMapping GetEntityMapping(Type entityType, IEntityMappingSource? mappingSource = null)
    {
        if (entityType is null)
            throw new ArgumentNullException(nameof(entityType));

        var source = mappingSource ?? DefaultEntityMappingSource;
        var mapping = source.GetMapping(entityType)
                      ?? throw new InvalidOperationException(
                          $"Mapping source '{source.GetType().FullName}' returned null for '{entityType.FullName}'.");
        if (mapping.EntityType != entityType)
        {
            throw new InvalidOperationException(
                $"Mapping source '{source.GetType().FullName}' " +
                $"returned a mapping for '{mapping.EntityType.FullName}' when '{entityType.FullName}' was requested.");
        }

        return mapping;
    }

    public static ISqlAdapter RegisterSqlAdapter(Type connectionType, ISqlAdapter adapter)
    {
        return _adapters[connectionType.FullName!] = adapter;
    }

    public static ISqlAdapter GetSqlAdapter(IDbConnection connection)
    {
        return _adapters.GetOrAdd(connection.GetType().FullName!, conName => throw new ArgumentException("Unsupported connection type: " + conName));
    }

    /// <summary>
    /// Gets the quoted table name, including an effective schema when supported by the adapter.
    /// </summary>
    /// <param name="sqlAdapter">The SQL dialect adapter.</param>
    /// <param name="schema">An optional schema overriding the mapping schema when non-null.</param>
    /// <param name="entityType">The mapped CLR entity type.</param>
    /// <param name="mappingSource">An optional entity mapping source.</param>
    /// <returns>The quoted table identifier.</returns>
    public static string GetTableNameWithSchema(
        ISqlAdapter sqlAdapter,
        string? schema,
        Type entityType,
        IEntityMappingSource? mappingSource = null)
    {
        return GetTableNameWithSchema(sqlAdapter, schema, GetEntityMapping(entityType, mappingSource));
    }

    internal static string GetTableNameWithSchema(ISqlAdapter sqlAdapter, string? schema, EntityMapping mapping)
    {
        var effectiveSchema = sqlAdapter.SupportsSchemas ? schema ?? mapping.Schema : null;
        return _tableFullNames.GetOrAdd((sqlAdapter, effectiveSchema, mapping), key =>
            key.Schema is null
                ? key.Adapter.GetQuotedTableName(key.Mapping.TableName)
                : $"{key.Adapter.GetQuotedTableName(key.Schema)}.{key.Adapter.GetQuotedTableName(key.Mapping.TableName)}");
    }

    /// <summary>
    /// Gets the quoted table name for a connection, including an effective schema when supported.
    /// </summary>
    /// <param name="connection">The connection used to resolve the SQL adapter.</param>
    /// <param name="schema">An optional schema overriding the mapping schema when non-null.</param>
    /// <param name="entityType">The mapped CLR entity type.</param>
    /// <param name="mappingSource">An optional entity mapping source.</param>
    /// <returns>The quoted table identifier.</returns>
    public static string GetTableNameWithSchema(
        IDbConnection connection,
        string? schema,
        Type entityType,
        IEntityMappingSource? mappingSource = null)
    {
        return GetTableNameWithSchema(GetSqlAdapter(connection), schema, entityType, mappingSource);
    }

    /// <summary>
    /// Gets a quoted column name from either its CLR property name or database column name.
    /// </summary>
    /// <param name="sqlAdapter">The SQL dialect adapter.</param>
    /// <param name="entityType">The mapped CLR entity type.</param>
    /// <param name="propertyOrColumnName">A CLR property name or database column name.</param>
    /// <param name="mappingSource">An optional entity mapping source.</param>
    /// <returns>The quoted database column name.</returns>
    /// <exception cref="ArgumentException">No mapped property or column has the supplied name.</exception>
    public static string GetQuotedColumnName(
        ISqlAdapter sqlAdapter,
        Type entityType,
        string propertyOrColumnName,
        IEntityMappingSource? mappingSource = null)
    {
        var mapping = GetEntityMapping(entityType, mappingSource);
        var property = mapping.FindProperty(propertyOrColumnName);
        return property is null
            ? throw new ArgumentException(
                $"Property or column '{propertyOrColumnName}' was not found in the mapping for '{entityType.FullName}'.",
                nameof(propertyOrColumnName))
            : sqlAdapter.GetQuotedColumnName(property.ColumnName);
    }

    /// <summary>
    /// Gets a quoted column name for a connection from either its CLR property name or database column name.
    /// </summary>
    /// <param name="connection">The connection used to resolve the SQL adapter.</param>
    /// <param name="entityType">The mapped CLR entity type.</param>
    /// <param name="propertyOrColumnName">A CLR property name or database column name.</param>
    /// <param name="mappingSource">An optional entity mapping source.</param>
    /// <returns>The quoted database column name.</returns>
    public static string GetQuotedColumnName(
        IDbConnection connection,
        Type entityType,
        string propertyOrColumnName,
        IEntityMappingSource? mappingSource = null)
    {
        return GetQuotedColumnName(GetSqlAdapter(connection), entityType, propertyOrColumnName, mappingSource);
    }

    /// <summary>
    /// Gets the quoted database column mapped to a selected CLR property.
    /// </summary>
    /// <typeparam name="T">The mapped CLR entity type.</typeparam>
    /// <param name="connection">The connection used to resolve the SQL adapter.</param>
    /// <param name="selector">An expression selecting one mapped property.</param>
    /// <param name="mappingSource">An optional entity mapping source.</param>
    /// <returns>The quoted database column name.</returns>
    public static string GetQuotedColumnName<T>(
        IDbConnection connection,
        Expression<Func<T, object?>> selector,
        IEntityMappingSource? mappingSource = null)
    {
        var member = Expression.GetMember(selector);
        return GetQuotedColumnName(GetSqlAdapter(connection), typeof(T), member.Name, mappingSource);
    }

    public static TransactionScope CreateAsyncTransactionScope(
        System.Transactions.IsolationLevel isolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
        TimeSpan? timeout = null)
    {
        var transactionOptions = new TransactionOptions
        {
            IsolationLevel = isolationLevel,
            Timeout = timeout ?? TransactionManager.MaximumTimeout,
        };
        return new TransactionScope(TransactionScopeOption.Required, transactionOptions, TransactionScopeAsyncFlowOption.Enabled);
    }
}
