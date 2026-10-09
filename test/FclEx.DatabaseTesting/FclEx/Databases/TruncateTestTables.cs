namespace FclEx.Databases;

/// <summary>Creates minimal isolated tables on demand, or leases fixture-initialized persistent tables for Oracle.</summary>
public sealed class TruncateTestTables(TestDatabaseEnvironment environment)
{
    private readonly Dictionary<string, SemaphoreSlim> _oracleCascadeLocks = new();
    private readonly Dictionary<string, (SemaphoreSlim Gate, ConcurrentQueue<string> Prefixes)> _oracleTables = new();

    /// <summary>Idempotently creates all dedicated Oracle tables for each selected target; other drivers create tables on demand.</summary>
    /// <remarks>Called by fixture initialization before any sessions are acquired.</remarks>
    public async Task InitializeAsync(IEnumerable<string?> schemas, CancellationToken cancellationToken = default)
    {
        if (!SelectedDrivers.Contains(DbDriver.Oracle))
            return;

        foreach (var (_, schema) in GetDriverSchemaCases(schemas).Where(pair => pair.Driver == DbDriver.Oracle))
        {
            using var connection = environment.Resolve(DbDriver.Oracle, schema).CreateConnection();
            await connection.OpenAsync(cancellationToken);
            using var session = new TruncateTestSession(connection, DbDriver.Oracle, schema, "TruncateCascade_");
            await CreateTableAsync(session, typeof(TruncateParent), false, true, cancellationToken);
            await CreateTableAsync(session, typeof(TruncateChild), false, true, cancellationToken);
            var prefixes = new[] { "Truncate_1_", "Truncate_2_" };
            foreach (var prefix in prefixes)
            {
                using var tableConnection = environment.Resolve(DbDriver.Oracle, schema).CreateConnection();
                await tableConnection.OpenAsync(cancellationToken);
                using var tableSession = new TruncateTestSession(tableConnection, DbDriver.Oracle, schema, prefix);
                foreach (var type in new[]
                         {
                             typeof(TruncateRow), typeof(TruncateOtherRow), typeof(TruncateManualRow),
                             typeof(TruncateKeylessRow), typeof(TruncateAttributedRow),
                         })
                    await CreateTableAsync(tableSession, type, false, true, cancellationToken);
            }
            if (!_oracleCascadeLocks.ContainsKey(schema ?? ""))
                _oracleCascadeLocks.Add(schema ?? "", new SemaphoreSlim(1, 1));
            if (!_oracleTables.ContainsKey(schema ?? ""))
                _oracleTables.Add(schema ?? "",
                    (new SemaphoreSlim(prefixes.Length, prefixes.Length), new ConcurrentQueue<string>(prefixes)));
        }
    }

    /// <summary>Creates the requested tables on other drivers, or leases persistent Oracle tables without executing DDL.</summary>
    /// <remarks>
    /// Oracle leases are exclusive and released on disposal; tables persist. Acquisition waits, with cancellation,
    /// when both row-table groups or the single cascade pair are in use. Other ordinary tables are dropped on disposal.
    /// </remarks>
    public async Task<TruncateTestSession> CreateSessionAsync(
        DbDriver driver,
        string? schema,
        IEnumerable<Type> entityTypes,
        DbConnection? connection = null,
        CancellationToken cancellationToken = default,
        bool ordinary = false)
    {
        var types = entityTypes.Distinct().ToArray();
        var oracleCascade = driver == DbDriver.Oracle && types.Contains(typeof(TruncateParent));
        Action? release = null;
        string? oraclePrefix = null;
        if (oracleCascade)
        {
            var gate = _oracleCascadeLocks[schema ?? ""];
            await gate.WaitAsync(cancellationToken);
            release = () => gate.Release();
        }
        else if (driver == DbDriver.Oracle)
        {
            var pool = _oracleTables[schema ?? ""];
            await pool.Gate.WaitAsync(cancellationToken);
            if (!pool.Prefixes.TryDequeue(out oraclePrefix))
            {
                pool.Gate.Release();
                throw new InvalidOperationException("No Oracle table prefix is available after acquiring a lease.");
            }
            var leasedPrefix = oraclePrefix;
            release = () =>
            {
                pool.Prefixes.Enqueue(leasedPrefix);
                pool.Gate.Release();
            };
        }

        var temporary = !ordinary && driver != DbDriver.Oracle;
        var prefix = oracleCascade ? "TruncateCascade_" : oraclePrefix ?? (temporary && driver == DbDriver.SqlServer ? "#" : "")
            + "Truncate_" + Guid.NewGuid().ToString("N").Substring(0, 12) + "_";
        TruncateTestSession? session = null;
        try
        {
            session = new TruncateTestSession(connection ?? environment.Resolve(driver, schema).CreateConnection(), driver,
                temporary ? driver switch { DbDriver.Npgsql => "pg_temp", DbDriver.SqlServer => null, _ => schema } : schema,
                prefix, release);
            await session.Connection.OpenAsync(cancellationToken);
            if (driver != DbDriver.Oracle)
            {
                foreach (var type in types)
                {
                    await CreateTableAsync(session, type, temporary, false, cancellationToken);
                    if (!temporary)
                        session.TablesToDrop.Add(session.GetQualifiedTableName(type));
                }
            }
            return session;
        }
        catch
        {
            if (session != null)
                session.Dispose();
            else
                release?.Invoke();
            throw;
        }
    }

    private static Task CreateTableAsync(
        TruncateTestSession session, Type entityType, bool temporary, bool ifNotExists, CancellationToken cancellationToken)
    {
        var driver = session.Driver;
        var adapter = DapperHelper.GetSqlAdapter(session.Connection);
        var integer = driver == DbDriver.Oracle ? "NUMBER(10)" : "INTEGER";
        var columns = new List<string>();
        if (entityType != typeof(TruncateKeylessRow))
        {
            var id = entityType == typeof(TruncateManualRow) ? integer + " PRIMARY KEY" : driver switch
            {
                DbDriver.SqlServer => "INTEGER IDENTITY(1,1) PRIMARY KEY",
                DbDriver.MySql or DbDriver.MySqlConnector => "INTEGER NOT NULL AUTO_INCREMENT PRIMARY KEY",
                DbDriver.Sqlite => "INTEGER PRIMARY KEY AUTOINCREMENT",
                _ => integer + " GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY",
            };
            columns.Add($"{Column("Id")} {id}");
        }
        if (entityType == typeof(TruncateRow) || entityType == typeof(TruncateOtherRow) || entityType == typeof(TruncateKeylessRow))
            columns.Add($"{Column("Value")} {integer} NOT NULL");
        if (entityType == typeof(TruncateChild))
        {
            columns.Add($"{Column("ParentId")} {integer} NOT NULL");
            columns.Add($"FOREIGN KEY ({Column("ParentId")}) REFERENCES {session.GetQualifiedTableName(typeof(TruncateParent))} ({Column("Id")}) ON DELETE CASCADE");
        }
        var create = temporary && driver != DbDriver.SqlServer ? "CREATE TEMPORARY TABLE" : "CREATE TABLE";
        var sql = $"{create} {session.GetQualifiedTableName(entityType)} ({string.Join(", ", columns)})";
        if (ifNotExists)
            sql = $"BEGIN EXECUTE IMMEDIATE '{sql.Replace("'", "''")}'; EXCEPTION WHEN OTHERS THEN IF SQLCODE != -955 THEN RAISE; END IF; END;";
        return session.Connection.ExecuteAsync(sql, cancellationToken: cancellationToken);

        string Column(string name) => adapter.GetQuotedColumnName(name);
    }
}
