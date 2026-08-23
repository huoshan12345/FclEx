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
    private static readonly ConditionalWeakTable<Type, EntityDefinition> _definitions = new();
    private static readonly ConcurrentDictionary<(Type AdapterType, string? Schema, Type EntityType), string> _tableFullNames = new();
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

    public static EntityDefinition GetEntityDefinition(Type type)
    {
        return _definitions.GetValue(type, EntityDefinition.GetDefinition);
    }

    internal static FclExDapperRegistration ApplyColumnMappings(
        IReadOnlyCollection<Type> entityTypes,
        DapperRegistrationConflictBehavior conflictBehavior)
    {
        if (conflictBehavior is not DapperRegistrationConflictBehavior.Throw
            and not DapperRegistrationConflictBehavior.KeepExisting
            and not DapperRegistrationConflictBehavior.Replace)
        {
            throw new ArgumentOutOfRangeException(nameof(conflictBehavior), conflictBehavior, null);
        }

        lock (_lock)
        {
            // Detect every conflict before mutating Dapper so Throw never leaves a partially applied configuration.
            foreach (var entityType in entityTypes)
            {
                if (TryGetActiveColumnMapping(entityType, out _))
                    continue;

                var currentMap = SqlMapper.GetTypeMap(entityType);
                if (currentMap is not DefaultTypeMap && conflictBehavior == DapperRegistrationConflictBehavior.Throw)
                {
                    throw new InvalidOperationException(
                        $"A custom Dapper type map is already registered for '{entityType.FullName}'. " +
                        $"Use {nameof(DapperRegistrationConflictBehavior)}.{nameof(DapperRegistrationConflictBehavior.KeepExisting)} " +
                        $"or {nameof(DapperRegistrationConflictBehavior)}.{nameof(DapperRegistrationConflictBehavior.Replace)} explicitly.");
                }
            }

            var registrations = new List<ColumnMappingRegistrationState>(entityTypes.Count);
            foreach (var entityType in entityTypes)
            {
                if (TryGetActiveColumnMapping(entityType, out var activeRegistration))
                {
                    activeRegistration.ReferenceCount++;
                    registrations.Add(activeRegistration);
                    continue;
                }

                // A stale entry means another component replaced our map while a registration was alive.
                _registrations.Remove(entityType);

                var currentMap = SqlMapper.GetTypeMap(entityType);
                if (currentMap is not DefaultTypeMap && conflictBehavior == DapperRegistrationConflictBehavior.KeepExisting)
                    continue;

                var map = new CustomPropertyTypeMap(entityType, (type, columnName) =>
                    GetEntityDefinition(type).Fields.FirstOrDefault(field => field.FieldName == columnName)?.PropertyInfo!);
                var registration = new ColumnMappingRegistrationState(
                    entityType,
                    currentMap is DefaultTypeMap ? null : currentMap,
                    map);

                SqlMapper.SetTypeMap(entityType, map);
                _registrations.Add(entityType, registration);
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
                if (!_registrations.TryGetValue(registration.EntityType, out var activeRegistration)
                    || !ReferenceEquals(activeRegistration, registration))
                {
                    continue;
                }

                registration.ReferenceCount--;
                if (registration.ReferenceCount > 0)
                    continue;

                _registrations.Remove(registration.EntityType);
                if (ReferenceEquals(SqlMapper.GetTypeMap(registration.EntityType), registration.AppliedMap))
                    SqlMapper.SetTypeMap(registration.EntityType, registration.PreviousMap);
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

    public static string GetTableNameWithSchema(ISqlAdapter sqlAdapter, string? schema, Type entityType)
    {
        return _tableFullNames.GetOrAdd((sqlAdapter.GetType(), schema, entityType), k =>
        {
            var tableName = GetEntityDefinition(k.EntityType).TableName;
            return k.Schema == null || sqlAdapter.SupportSchema == false
                ? sqlAdapter.GetQuotedTableName(tableName)
                : $"{sqlAdapter.GetQuotedTableName(k.Schema)}.{sqlAdapter.GetQuotedTableName(tableName)}";
        });
    }

    public static string GetTableNameWithSchema(IDbConnection connection, string? schema, Type entityType)
    {
        return GetTableNameWithSchema(GetSqlAdapter(connection), schema, entityType);
    }

    public static string GetQuotedColumnName(ISqlAdapter sqlAdapter, Type entityType, string columnName)
    {
        var entityDef = GetEntityDefinition(entityType);
        var fieldDef = entityDef.Fields.FirstOrDefault(f => f.FieldName == columnName);
        return fieldDef == null
            ? throw new ArgumentException($"Column '{columnName}' not found in entity '{entityType.FullName}'.")
            : sqlAdapter.GetQuotedColumnName(fieldDef.FieldName);
    }

    public static string GetQuotedColumnName(IDbConnection connection, Type entityType, string columnName)
    {
        return GetQuotedColumnName(GetSqlAdapter(connection), entityType, columnName);
    }

    public static string GetQuotedColumnName<T>(IDbConnection connection, Expression<Func<T, object?>> selector)
    {
        var member = Expression.GetMember(selector);
        return GetQuotedColumnName(GetSqlAdapter(connection), typeof(T), member.Name);
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
