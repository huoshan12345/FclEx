namespace FclEx.Databases;

/// <summary>Resolves test namespaces and logins and owns a disposable SQLite database.</summary>
public sealed class TestDatabaseEnvironment(
    DatabasesConfig configuration,
    string databaseName,
    DatabaseUser defaultUser) : IDisposable
{
    private string? _sqliteDirectory;
    public DatabaseUser DefaultUser { get; } = defaultUser;

    public TestDatabaseTarget Resolve(DbDriver driver, string? schema = null, TestLogin login = TestLogin.Standard)
    {
        if (driver == DbDriver.Sqlite)
        {
            var directory = _sqliteDirectory ?? throw new InvalidOperationException("Initialize SQLite before creating connections.");
            return new(driver, new(), FilePath: Path.Combine(directory, "test.sqlite"));
        }

        var configurationForDriver = driver switch
        {
            DbDriver.SqlServer => configuration.SqlServer,
            DbDriver.Npgsql => configuration.Postgres,
            DbDriver.MySql or DbDriver.MySqlConnector => configuration.MySql,
#if SUPPORT_ORACLE
            DbDriver.Oracle => configuration.Oracle,
#endif
            _ => throw new ArgumentOutOfRangeException(nameof(driver), driver, null),
        };

        var credentials = login switch
        {
            TestLogin.Standard => configurationForDriver,
            TestLogin.DefaultSchemaUser => configurationForDriver with
            {
                UserName = DefaultUser.UserName,
                Password = driver == DbDriver.SqlServer ? SqlServerUserPassword : DefaultUser.Password,
            },
            _ => throw new ArgumentOutOfRangeException(nameof(login), login, null),
        };

        if (driver == DbDriver.Oracle)
        {
            var owner = login == TestLogin.DefaultSchemaUser ? DefaultUser.UserName : schema ?? databaseName;
            return new(driver, credentials with { UserName = owner }, Schema: schema);
        }

        var database = driver.IsMySql() ? schema ?? databaseName : databaseName;
        return new(driver, credentials, database, schema);
    }

    public async Task InitializeSqliteAsync(CancellationToken cancellationToken = default)
    {
        if (_sqliteDirectory is not null)
            throw new InvalidOperationException("This environment has already initialized SQLite.");

        _sqliteDirectory = Path.Combine(Path.GetTempPath(), "FclEx.DatabaseTesting", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_sqliteDirectory);
        try
        {
            using var connection = Resolve(DbDriver.Sqlite).CreateConnection();
            await connection.OpenAsync(cancellationToken);
            using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA journal_mode=WAL;";
            await command.ExecuteNonQueryAsync(cancellationToken);
            command.CommandText = ReadSqliteSchema();
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    public static string ReadSqliteSchema()
    {
        using var stream = typeof(TestDatabaseEnvironment).Assembly.GetManifestResourceStream("FclEx.DatabaseTesting.Sqlite.sql")
            ?? throw new InvalidOperationException("The generated SQLite schema resource is missing.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public void Dispose()
    {
        if (_sqliteDirectory is not null)
        {
            Directory.Delete(_sqliteDirectory, recursive: true);
            _sqliteDirectory = null;
        }
    }
}
