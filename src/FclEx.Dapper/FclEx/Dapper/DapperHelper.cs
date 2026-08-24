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
    private static readonly Dictionary<Type, ColumnMappingRegistrationState> _registrations = new();

    private static readonly
#if NET9_0_OR_GREATER
        Lock
#else
        object
#endif
    _lock = new();

    /// <summary>
    /// Creates an explicit configuration builder for FclEx-owned Dapper type maps.
    /// </summary>
    /// <remarks>
    /// Creating or otherwise accessing <see cref="DapperHelper"/> does not scan assemblies or modify
    /// Dapper's process-wide type maps. Call <see cref="FclExDapperConfigurationBuilder.Apply"/> to apply
    /// the selected mappings and retain the returned registration for as long as they are required.
    /// </remarks>
    /// <returns>A new configuration builder with no mappings selected.</returns>
    public static FclExDapperConfigurationBuilder CreateConfiguration()
    {
        return new FclExDapperConfigurationBuilder();
    }

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

    internal static FclExDapperRegistration ApplyColumnMappings(
        IReadOnlyCollection<Type> entityTypes,
        IEntityMappingSource mappingSource,
        DapperRegistrationConflictBehavior conflictBehavior)
    {
        if (conflictBehavior is not DapperRegistrationConflictBehavior.Throw
            and not DapperRegistrationConflictBehavior.KeepExisting
            and not DapperRegistrationConflictBehavior.Replace)
        {
            throw new ArgumentOutOfRangeException(nameof(conflictBehavior), conflictBehavior, null);
        }

        var mappings = entityTypes.Select(entityType => GetEntityMapping(entityType, mappingSource)).ToArray();

        lock (_lock)
        {
            // Detect every conflict before mutating Dapper so Throw never leaves a partially applied configuration.
            foreach (var mapping in mappings)
            {
                if (TryGetActiveColumnMapping(mapping.EntityType, out var activeRegistration)
                    && ReferenceEquals(activeRegistration.Mapping, mapping))
                {
                    continue;
                }

                var currentMap = SqlMapper.GetTypeMap(mapping.EntityType);
                if (currentMap is not DefaultTypeMap && conflictBehavior == DapperRegistrationConflictBehavior.Throw)
                {
                    throw new InvalidOperationException(
                        $"A custom Dapper type map is already registered for '{mapping.EntityType.FullName}'. " +
                        $"Use {nameof(DapperRegistrationConflictBehavior)}.{nameof(DapperRegistrationConflictBehavior.KeepExisting)} " +
                        $"or {nameof(DapperRegistrationConflictBehavior)}.{nameof(DapperRegistrationConflictBehavior.Replace)} explicitly.");
                }
            }

            var registrations = new List<ColumnMappingRegistrationState>(mappings.Length);
            foreach (var mapping in mappings)
            {
                if (TryGetActiveColumnMapping(mapping.EntityType, out var activeRegistration)
                    && ReferenceEquals(activeRegistration.Mapping, mapping))
                {
                    activeRegistration.ReferenceCount++;
                    registrations.Add(activeRegistration);
                    continue;
                }

                var hasPreviousRegistration = TryGetActiveColumnMapping(mapping.EntityType, out var previousRegistration);
                var currentMap = SqlMapper.GetTypeMap(mapping.EntityType);
                if (currentMap is not DefaultTypeMap && conflictBehavior == DapperRegistrationConflictBehavior.KeepExisting)
                    continue;

                var map = new CustomPropertyTypeMap(mapping.EntityType, (_, identifier) =>
                    mapping.FindProperty(identifier)?.Property!);
                var registration = new ColumnMappingRegistrationState(
                    mapping,
                    currentMap is DefaultTypeMap ? null : currentMap,
                    map,
                    hasPreviousRegistration ? previousRegistration : null);

                SqlMapper.SetTypeMap(mapping.EntityType, map);
                _registrations[mapping.EntityType] = registration;
                registrations.Add(registration);
            }

            return new FclExDapperRegistration(registrations);
        }
    }

    internal static void ReleaseColumnMappings(IReadOnlyCollection<ColumnMappingRegistrationState> registrations)
    {
        lock (_lock)
        {
            foreach (var registration in registrations)
            {
                registration.ReferenceCount--;
                if (registration.ReferenceCount > 0)
                    continue;

                if (!_registrations.TryGetValue(registration.Mapping.EntityType, out var activeRegistration)
                    || !ReferenceEquals(activeRegistration, registration))
                {
                    continue;
                }

                if (!ReferenceEquals(SqlMapper.GetTypeMap(registration.Mapping.EntityType), registration.AppliedMap))
                {
                    _registrations.Remove(registration.Mapping.EntityType);
                    continue;
                }

                var stateToRestore = registration;
                var previousRegistration = stateToRestore.PreviousRegistration;
                while (previousRegistration is not null && previousRegistration.ReferenceCount == 0)
                {
                    stateToRestore = previousRegistration;
                    previousRegistration = stateToRestore.PreviousRegistration;
                }

                var previousMap = previousRegistration?.AppliedMap ?? stateToRestore.PreviousMap;
                SqlMapper.SetTypeMap(registration.Mapping.EntityType, previousMap);

                if (previousRegistration is null)
                    _registrations.Remove(registration.Mapping.EntityType);
                else
                    _registrations[registration.Mapping.EntityType] = previousRegistration;
            }
        }
    }

    private static bool TryGetActiveColumnMapping(Type entityType, out ColumnMappingRegistrationState registration)
    {
        return _registrations.TryGetValue(entityType, out registration!)
               && ReferenceEquals(SqlMapper.GetTypeMap(entityType), registration.AppliedMap);
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
        var effectiveSchema = sqlAdapter.SupportSchema ? schema ?? mapping.Schema : null;
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
