using System.Transactions;

namespace FclEx.Dapper;

public static class DapperHelper
{
    private static volatile bool _isDapperInitialized = false;

    static DapperHelper()
    {
        Initialize();
    }

    internal static readonly ConcurrentDictionary<string, ISqlAdapter> Adapters = new()
    {
        ["Npgsql.NpgsqlConnection"] = NpgsqlAdapter.Instance,
        ["Microsoft.Data.SqlClient.SqlConnection"] = SqlServerAdapter.Instance,
        ["Microsoft.Data.Sqlite.SqliteConnection"] = SqliteAdapter.Instance,
        ["MySql.Data.MySqlClient.MySqlConnection"] = MySqlAdapter.Instance,
        ["MySqlConnector.MySqlConnection"] = MySqlConnectorAdapter.Instance,
    };
    internal static readonly ConditionalWeakTable<Type, EntityDefinition> EntityDefinitions = new();
    internal static readonly ConcurrentDictionary<(Type AdapterType, string? Schema, Type EntityType), string> TableNamesWithSchema = new();

    internal class AssemblyLocker
    {
        public
#if NET9_0_OR_GREATER
            Lock
#else
            object
#endif
            LockObj
        { get; } = new();

        public bool Initialized { get; set; } = false;
    }
    internal static readonly ConcurrentDictionary<Assembly, AssemblyLocker> Lockers = new();

    public static EntityDefinition GetEntityDefinition(Type type)
    {
        return EntityDefinitions.GetValue(type, EntityDefinition.GetDefinition);
    }

    // Register CustomPropertyTypeMap for Type with ColumnAttribute
    // And make column name case-insensitive.
    public static void RegisterColumnMapping(params Type[] types)
    {
        if (types.IsNullOrEmpty())
            return;

        foreach (var entityType in types)
        {
            var map = new CustomPropertyTypeMap(entityType, (t, name) =>
                GetEntityDefinition(t).Fields.FirstOrDefault(p => p.FieldName == name)?.PropertyInfo!);
            SqlMapper.SetTypeMap(entityType, map);
        }
    }

    public static void Initialize(Assembly assembly)
    {
        var locker = Lockers.GetOrAdd(assembly, m => new());

        if (locker.Initialized)
            return;

        lock (locker.LockObj)
        {
            if (locker.Initialized)
                return;

            if (assembly.GetName().Name?.StartsWith("Microsoft.TestPlatform.") == true)
            {
                // Skip test platform assemblies to avoid the error "Could not load type 'System.Diagnostics.CodeAnalysis.MemberNotNullWhenAttribute' from assembly 'Microsoft.TestPlatform.CoreUtilities'".
                locker.Initialized = true;
                return;
            }

            var types = assembly.ExportedTypes.ToList();
            var typesWithColumn = types.Where(m => m.GetCustomAttribute<TableAttribute>() != null
                                                   || m.GetProperties().Any(x => x.GetCustomAttribute<ColumnAttribute>() != null)).ToArray();
            RegisterColumnMapping(typesWithColumn);

            locker.Initialized = true;
        }
    }

    public static void Initialize()
    {
        if (_isDapperInitialized)
            return;

        _isDapperInitialized = true;

        SqlMapper.AddTypeHandler(new GuidTypeHandler());
        //SqlMapper.AddTypeHandler(new DateTimeHandler());

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(p => !p.IsDynamic))
        {
            Initialize(assembly);
        }
    }

    public static ISqlAdapter RegisterSqlAdapter(Type connectionType, ISqlAdapter adapter)
    {
        return Adapters[connectionType.FullName!] = adapter;
    }

    public static ISqlAdapter GetSqlAdapter(IDbConnection connection)
    {
        return Adapters.GetOrAdd(connection.GetType().FullName!, conName => throw new ArgumentException("Unsupported connection type: " + conName));
    }
    
    public static string GetTableNameWithSchema(ISqlAdapter sqlAdapter, string? schema, Type entityType)
    {
        return TableNamesWithSchema.GetOrAdd((sqlAdapter.GetType(), schema, entityType), k =>
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

    public static TransactionScope CreateAsyncTransactionScope(System.Transactions.IsolationLevel isolationLevel = System.Transactions.IsolationLevel.ReadCommitted)
    {
        var transactionOptions = new TransactionOptions
        {
            IsolationLevel = isolationLevel,
            Timeout = TransactionManager.MaximumTimeout
        };
        return new TransactionScope(TransactionScopeOption.Required, transactionOptions, TransactionScopeAsyncFlowOption.Enabled);
    }
}