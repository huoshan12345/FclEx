// ReSharper disable UseAwaitUsing

// ReSharper disable LoopCanBeConvertedToQuery
namespace FclEx.Databases;

/// <summary>Creates connection-local tables and pre-creates reusable Oracle tables for truncate tests.</summary>
public sealed class TruncateTestTables(TestDatabaseEnvironment environment)
{
    private const int OracleTableGroupCount = 4;
    private readonly Dictionary<string, OracleTablePool> _oraclePools = new();
    private static readonly Type[] EntityTypes =
    [
        typeof(EntityWithAutoKey),
        typeof(EntityWithIdAndIndex),
        typeof(EntityWithGuidKey),
        typeof(EntityWithoutKey),
        typeof(HasTableAttributeEntity),
        typeof(EntityHasStates),
        typeof(EntityWithNavigation),
    ];

    /// <summary>Initializes only selected targets. Existing tables are retained between test runs.</summary>
    public async Task InitializeAsync(IEnumerable<string?> schemas, CancellationToken cancellationToken = default)
    {
        foreach (var (driver, schema) in GetDriverSchemaCases(schemas))
        {
            if (driver == DbDriver.MySql)
                continue;

            using var connection = environment.Resolve(driver, schema).CreateConnection();
            await connection.OpenAsync(cancellationToken);
            // One test owns this table per target/project, preserving ordinary schema and connection-state coverage.
            using var stateSession = new TruncateTestSession(connection, driver, schema, "TruncateClosed_");
            await CreateTableAsync(stateSession, typeof(EntityWithAutoKey), temporary: false, cancellationToken);
            if (driver != DbDriver.Oracle)
                continue;

            var pool = new OracleTablePool();
            for (var slot = 0; slot < OracleTableGroupCount; slot++)
            {
                var prefix = $"Truncate{slot}_";
                var session = new TruncateTestSession(connection, driver, schema, prefix);
                foreach (var entityType in EntityTypes)
                    await CreateTableAsync(session, entityType, temporary: false, cancellationToken);
                pool.Available.Enqueue(prefix);
            }
            _oraclePools.Add(schema ?? "", pool);
        }
    }

    /// <summary>Leases Oracle tables or creates the requested temporary tables on a new open connection.</summary>
    public async Task<TruncateTestSession> CreateSessionAsync(
        DbDriver driver,
        string? schema,
        IEnumerable<Type> entityTypes,
        DbConnection? connection = null,
        CancellationToken cancellationToken = default)
    {
        string prefix;
        Action? release = null;
        if (driver == DbDriver.Oracle)
        {
            var pool = _oraclePools[schema ?? ""];
            await pool.Ready.WaitAsync(cancellationToken);
            if (!pool.Available.TryDequeue(out prefix!))
                throw new InvalidOperationException("No Oracle table group is available after acquiring its lease.");
            release = () =>
            {
                pool.Available.Enqueue(prefix);
                pool.Ready.Release();
            };
        }
        else
        {
            prefix = driver == DbDriver.SqlServer ? "#" : "";
        }

        TruncateTestSession? session = null;
        try
        {
            session = new TruncateTestSession(connection ?? environment.Resolve(driver, schema).CreateConnection(), driver,
                driver switch { DbDriver.Npgsql => "pg_temp", DbDriver.SqlServer => null, _ => schema }, prefix, release);

            await session.Connection.OpenAsync(cancellationToken);

            if (driver == DbDriver.Oracle)
                return session;

            foreach (var entityType in entityTypes.Distinct())
                await CreateTableAsync(session, entityType, temporary: true, cancellationToken);
            return session;
        }
        catch
        {
            if (session is not null)
                session.Dispose();
            else
                release?.Invoke();
            throw;
        }
    }

    /// <summary>Returns the ordinary table owned by one schema or closed-connection test per target and project.</summary>
    public TruncateTestSession CreateClosedConnectionSession(DbDriver driver, string? schema)
        => new(environment.Resolve(driver, schema).CreateConnection(), driver, schema, "TruncateClosed_");

    private static async Task CreateTableAsync(
        TruncateTestSession session, Type entityType, bool temporary, CancellationToken cancellationToken)
    {
        var driver = session.Driver;
        var adapter = DapperHelper.GetSqlAdapter(session.Connection);
        var table = session.GetQualifiedTableName(entityType);
        var integer = driver == DbDriver.Oracle ? "NUMBER(10)" : "INTEGER";
        var longInteger = driver == DbDriver.Oracle ? "NUMBER(19)" : driver == DbDriver.Sqlite ? "INTEGER" : "BIGINT";
        var text = driver switch
        {
            DbDriver.SqlServer => "nvarchar(100)",
            DbDriver.Oracle => "NVARCHAR2(100)",
            DbDriver.Sqlite => "TEXT",
            _ => "varchar(100)",
        };
        string Identity(string type) => driver switch
        {
            DbDriver.SqlServer => $"{type} IDENTITY(1,1) PRIMARY KEY",
            DbDriver.MySql or DbDriver.MySqlConnector => $"{type} NOT NULL AUTO_INCREMENT PRIMARY KEY",
            DbDriver.Sqlite => "INTEGER PRIMARY KEY AUTOINCREMENT",
            _ => $"{type} GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY",
        };
        var columns = new List<string>();
        if (entityType != typeof(EntityWithoutKey))
        {
            var id = entityType == typeof(EntityWithGuidKey)
                ? driver switch
                {
                    DbDriver.SqlServer => "uniqueidentifier PRIMARY KEY",
                    DbDriver.Npgsql => "uuid PRIMARY KEY",
                    DbDriver.Oracle => "RAW(16) PRIMARY KEY",
                    DbDriver.Sqlite => "TEXT PRIMARY KEY",
                    _ => "char(36) PRIMARY KEY",
                }
                : Identity(entityType == typeof(EntityHasStates) || entityType == typeof(EntityWithNavigation) ? longInteger : integer);
            columns.Add($"{Column("Id")} {id}");
        }
        if (entityType != typeof(HasTableAttributeEntity) && entityType != typeof(EntityWithGuidKey))
        {
            columns.Add($"{Column("Name")} {text}" + (entityType == typeof(EntityWithIdAndIndex) ? " NOT NULL UNIQUE"
                : entityType == typeof(EntityHasStates) || entityType == typeof(EntityWithNavigation) ? " NOT NULL"
                : " NULL"));
        }
        if (entityType == typeof(EntityWithAutoKey) || entityType == typeof(EntityWithIdAndIndex)
            || entityType == typeof(EntityWithGuidKey) || entityType == typeof(EntityWithoutKey))
        {
            columns.Add($"{Column("Value")} {integer} NOT NULL");
        }
        if (entityType == typeof(EntityWithGuidKey))
        {
            columns.Add($"{Column("Order")} {integer} NULL");
        }
        if (entityType == typeof(EntityHasStates))
        {
            var timestamp = driver == DbDriver.Oracle ? "TIMESTAMP WITH TIME ZONE" : "timestamp with time zone";
            foreach (var name in new[] { "CreatedAt", "UpdatedAt", "DeletedAt" })
            {
                columns.Add($"{Column(name)} {timestamp} NOT NULL");
            }
            foreach (var name in new[] { "IsDisabled", "IsDeleted" })
            {
                columns.Add($"{Column(name)} BOOLEAN NOT NULL");
            }
        }
        if (entityType == typeof(EntityWithNavigation))
        {
            columns.Add($"{Column("NavigationId")} {longInteger} NULL");
            columns.Add($"FOREIGN KEY ({Column("NavigationId")}) REFERENCES {session.GetQualifiedTableName(typeof(EntityHasStates))} ({Column("Id")}) ON DELETE CASCADE");
        }
        var create = temporary && driver != DbDriver.SqlServer ? "CREATE TEMPORARY TABLE" : "CREATE TABLE";
        if (!temporary && driver != DbDriver.SqlServer)
            create += " IF NOT EXISTS";
        var sql = $"{create} {table} ({string.Join(", ", columns)})";
        if (!temporary && driver == DbDriver.SqlServer)
            sql = $"IF OBJECT_ID(N'{table.Replace("'", "''")}', N'U') IS NULL {sql}";
        await session.Connection.ExecuteAsync(sql, cancellationToken: cancellationToken);
        return;

        string Column(string name) => adapter.GetQuotedColumnName(name);
    }

    private sealed class OracleTablePool
    {
        public ConcurrentQueue<string> Available { get; } = new();
        public SemaphoreSlim Ready { get; } = new(OracleTableGroupCount, OracleTableGroupCount);
    }
}
