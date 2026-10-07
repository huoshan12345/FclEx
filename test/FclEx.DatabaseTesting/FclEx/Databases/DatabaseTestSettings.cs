namespace FclEx.Databases;

/// <summary>Defines run selection separately from the remote provisioning matrix.</summary>
public static class DatabaseTestSettings
{
    public const string ProviderEnvironmentVariable = "FCLEX_TEST_DATABASES";
    public const string DbName = "test";
    public const string DefaultUserName = "user";
    public const string DefaultUserPassword = "123456";
    public const string SqlServerUserPassword = "0Im1lI9BRZur";
    public const string UserSchema = "schema";
    public static readonly string?[] SchemaNames = [null, UserSchema];

    public static readonly DbDriver[] SupportedDrivers =
    [
        DbDriver.SqlServer, 
        DbDriver.Sqlite,
        DbDriver.Npgsql, 
        DbDriver.MySql,
        DbDriver.MySqlConnector,
#if SUPPORT_ORACLE
        DbDriver.Oracle,
#endif
    ];

    public static readonly DbDriver[] ProvisioningDrivers = SupportedDrivers
        .Where(driver => driver is not DbDriver.Sqlite and not DbDriver.MySql).ToArray();

    public static readonly DbDriver[] DbDrivers = SelectDrivers(
        Environment.GetEnvironmentVariable(ProviderEnvironmentVariable),
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTION")),
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows));

    public static DbDriver[] SelectDrivers(string? selection, bool isGithubAction, bool isWindows)
    {
        if (selection is null)
        {
            return isGithubAction
                ? isWindows
                    ? [
                        DbDriver.Npgsql,
                        DbDriver.Sqlite
                    ]
                    : SupportedDrivers
                : [
                    DbDriver.SqlServer,
                    DbDriver.Sqlite,
                ];
        }

        var drivers = new List<DbDriver>();
        foreach (var name in selection.Split(','))
        {
            if (!Enum.TryParse<DbDriver>(name.Trim(), true, out var driver)
                || !SupportedDrivers.Contains(driver)
                || !string.Equals(name.Trim(), driver.ToString(), StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"Unsupported database driver '{name}'. Supported drivers: {string.Join(", ", SupportedDrivers)}.", nameof(selection));

            if (!drivers.Contains(driver))
                drivers.Add(driver);
        }
        return drivers.ToArray();
    }

    public static IEnumerable<(DbDriver Driver, string? Schema)> SchemaCases(IEnumerable<string?> schemas)
    {
        foreach (var driver in DbDrivers)
        foreach (var schema in schemas)
        {
            if (driver != DbDriver.Sqlite || schema is null)
                yield return (driver, schema);
        }
    }

    public static Task<int> FixAutoIncrement<T>(IDbConnection connection, DbDriver driver, string? schema)
    {
        if (driver != DbDriver.Npgsql)
            return Task.FromResult(0);

        var table = DapperHelper.GetTableNameWithSchema(connection, schema, typeof(T));
        return connection.ExecuteScalarAsync<int>($"""
            SELECT setval(
                pg_get_serial_sequence('{table}', 'Id'),
                COALESCE((SELECT MAX("Id") FROM {table}), 0) + 1,
                false
            );
            """);
    }

    public static DbParameter CreateParameter(DbDriver driver, string name, object value) => driver switch
    {
        DbDriver.SqlServer => new Microsoft.Data.SqlClient.SqlParameter(name, value),
        DbDriver.Sqlite => new SqliteParameter(name, value),
        DbDriver.Npgsql => new Npgsql.NpgsqlParameter(name, value),
        DbDriver.MySql => new MySql.Data.MySqlClient.MySqlParameter(name, value),
        DbDriver.MySqlConnector => new MySqlConnector.MySqlParameter(name, value),
#if SUPPORT_ORACLE
        DbDriver.Oracle => new Oracle.ManagedDataAccess.Client.OracleParameter(name, value is Guid guid ? guid.ToByteArray() : value),
#endif
        _ => throw new ArgumentOutOfRangeException(nameof(driver), driver, null),
    };
}
