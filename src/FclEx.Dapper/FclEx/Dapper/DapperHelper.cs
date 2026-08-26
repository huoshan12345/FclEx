namespace FclEx.Dapper;

public static class DapperHelper
{
    private static readonly ConcurrentDictionary<Type, ISqlAdapter> RegisteredAdapters = new();
    private static readonly IReadOnlyDictionary<(string AssemblyName, string TypeName), ISqlAdapter> BuiltInAdapters =
        new Dictionary<(string AssemblyName, string TypeName), ISqlAdapter>
    {
        [("Npgsql", "Npgsql.NpgsqlConnection")] = new NpgsqlAdapter(),
        [("Microsoft.Data.SqlClient", "Microsoft.Data.SqlClient.SqlConnection")] = new SqlServerAdapter(),
        [("Microsoft.Data.Sqlite", "Microsoft.Data.Sqlite.SqliteConnection")] = new SqliteAdapter(),
        [("MySql.Data", "MySql.Data.MySqlClient.MySqlConnection")] = new MySqlAdapter(),
        [("MySqlConnector", "MySqlConnector.MySqlConnection")] = new MySqlConnectorAdapter(),
    };

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

    public static void RegisterSqlAdapter(Type connectionType, ISqlAdapter adapter)
    {
        if (connectionType is null)
            throw new ArgumentNullException(nameof(connectionType));
        if (adapter is null)
            throw new ArgumentNullException(nameof(adapter));
        if (!typeof(IDbConnection).IsAssignableFrom(connectionType))
        {
            throw new ArgumentException(
                $"'{connectionType.FullName}' does not implement {nameof(IDbConnection)}.",
                nameof(connectionType));
        }

        ISqlAdapter? replacedAdapter = null;
        RegisteredAdapters.AddOrUpdate(
            connectionType,
            adapter,
            (_, current) =>
            {
                replacedAdapter = current;
                return adapter;
            });

        if (replacedAdapter is not null
            && !ReferenceEquals(replacedAdapter, adapter)
            && !RegisteredAdapters.Values.Any(registered => ReferenceEquals(registered, replacedAdapter)))
        {
            DbConnectionExtensions.RemoveSqlCacheEntries(replacedAdapter);
        }
    }

    public static void RegisterSqlAdapter<TConnection>(ISqlAdapter adapter)
        where TConnection : IDbConnection
    {
        RegisterSqlAdapter(typeof(TConnection), adapter);
    }

    public static ISqlAdapter GetSqlAdapter(IDbConnection connection)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        var connectionType = connection.GetType();
        var registeredAdapter = GetRegisteredSqlAdapter(connectionType);
        if (registeredAdapter is not null)
            return registeredAdapter;

        for (var type = connectionType; type is not null; type = type.BaseType)
        {
            var assemblyName = type.Assembly.GetName().Name;
            var typeName = type.FullName;
            if (assemblyName is not null
                && typeName is not null
                && BuiltInAdapters.TryGetValue((assemblyName, typeName), out var builtInAdapter))
            {
                return builtInAdapter;
            }
        }

        throw new NotSupportedException(
            $"No SQL adapter is registered for connection type '{connectionType.AssemblyQualifiedName}'.");
    }

    private static ISqlAdapter? GetRegisteredSqlAdapter(Type connectionType)
    {
        if (RegisteredAdapters.TryGetValue(connectionType, out var exactAdapter))
            return exactAdapter;
        if (RegisteredAdapters.IsEmpty)
            return null;

        List<KeyValuePair<Type, ISqlAdapter>>? mostSpecificAdapters = null;
        foreach (var candidate in RegisteredAdapters)
        {
            if (!candidate.Key.IsAssignableFrom(connectionType))
                continue;

            mostSpecificAdapters ??= [];
            var candidateIsLessSpecific = false;
            for (var i = mostSpecificAdapters.Count - 1; i >= 0; i--)
            {
                var existing = mostSpecificAdapters[i];
                if (candidate.Key.IsAssignableFrom(existing.Key))
                {
                    candidateIsLessSpecific = true;
                    break;
                }

                if (existing.Key.IsAssignableFrom(candidate.Key))
                    mostSpecificAdapters.RemoveAt(i);
            }

            if (!candidateIsLessSpecific)
                mostSpecificAdapters.Add(candidate);
        }

        if (mostSpecificAdapters is null)
            return null;
        if (mostSpecificAdapters.Count == 1)
            return mostSpecificAdapters[0].Value;

        var registrations = string.Join(", ", mostSpecificAdapters
            .Select(candidate => candidate.Key.AssemblyQualifiedName)
            .OrderBy(name => name, StringComparer.Ordinal));
        throw new InvalidOperationException(
            $"Multiple SQL adapter registrations match connection type '{connectionType.AssemblyQualifiedName}': {registrations}.");
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

        // Complete CRUD command texts already cache the quoted table name on the stable path. Keeping a second
        // process-wide table-name cache would retain every per-call schema override and ad-hoc adapter indefinitely.
        return effectiveSchema is null
            ? sqlAdapter.GetQuotedTableName(mapping.TableName)
            : $"{sqlAdapter.GetQuotedTableName(effectiveSchema)}.{sqlAdapter.GetQuotedTableName(mapping.TableName)}";
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
