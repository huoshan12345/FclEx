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
        ["NpgsqlConnection"] = NpgsqlAdapter.Instance,
        ["SqlConnection"] = SqlServerAdapter.Instance,
    };
    internal static readonly ConcurrentDictionary<Type, EntityDefinition> EntityDefinitions = new();
    internal static readonly ConcurrentDictionary<(Type AdapterType, string Schema, Type EntityType), string> TableNamesWithSchema = new();

    internal class AssemblyLocker
    {
        public readonly object LockObj = new();
        public bool Initialized { get; set; } = false;
    }
    internal static readonly ConcurrentDictionary<Assembly, AssemblyLocker> Lockers = new();

    public static EntityDefinition GetEntityDefinition(Type type)
    {
        return EntityDefinitions.GetOrAdd(type, t => EntityDefinition.GetDefinition(t));
    }

    // Register CustomPropertyTypeMap for Type with ColumnAttribute
    // And make column name case insensitive.
    public static void RegisterColumnMapping(params Type[] types)
    {
        if (types.IsNullOrEmpty())
            return;

        foreach (var entityType in types)
        {
            var map = new CustomPropertyTypeMap(entityType, (t, name) => GetEntityDefinition(t).Fields.FirstOrDefault(p => p.FieldName == name)?.PropertyInfo);
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

            var types = assembly.ExportedTypes.ToList();

            var typesWithColumn = types.Where(m => m.GetProperties().Any(x => x.GetCustomAttribute<ColumnAttribute>() != null)).ToArray();
            RegisterColumnMapping(typesWithColumn);

            locker.Initialized = true;
        }
    }

    public static void Initialize()
    {
        if (_isDapperInitialized)
            return;

        _isDapperInitialized = true;

        //SqlMapper.AddTypeHandler(new DateTimeHandler());

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(p => !p.IsDynamic))
        {
            Initialize(assembly);
        }
    }

    public static ISqlAdapter GetSqlAdapter(IDbConnection connection)
    {
        return Adapters.GetOrAdd(connection.GetType().Name, conName => NpgsqlAdapter.Instance); // use postgres as default
    }

    public static string GetTableNameWithSchema(ISqlAdapter sqlAdapter, string schema, Type entityType)
    {
        return TableNamesWithSchema.GetOrAdd((sqlAdapter.GetType(), schema, entityType), k =>
        {
            var tableName = GetEntityDefinition(k.EntityType).TableName;
            return $"{sqlAdapter.GetQuotedTableName(k.Schema)}.{sqlAdapter.GetQuotedTableName(tableName)}";
        });
    }

    public static TransactionScope CreateAsyncTransactionScope(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        var transactionOptions = new TransactionOptions
        {
            IsolationLevel = isolationLevel,
            Timeout = TransactionManager.MaximumTimeout
        };
        return new TransactionScope(TransactionScopeOption.Required, transactionOptions, TransactionScopeAsyncFlowOption.Enabled);
    }
}