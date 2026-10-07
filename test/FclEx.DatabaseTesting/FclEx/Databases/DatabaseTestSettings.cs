using FclEx.Xunit;

namespace FclEx.Databases;

/// <summary>Defines run selection separately from the remote provisioning matrix.</summary>
public static class DatabaseTestSettings
{
    public const string DriverSelectionEnvironmentVariable = "FCLEX_TEST_DATABASES";
    public const string DatabaseName = "test";
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
        .Where(driver => driver is not DbDriver.Sqlite and not DbDriver.MySql)
        .ToArray();

    /// <summary>The drivers selected once for this test process, using the override or host defaults.</summary>
    public static readonly DbDriver[] SelectedDrivers = SelectDrivers(
        Environment.GetEnvironmentVariable(DriverSelectionEnvironmentVariable),
        TestHelper.IsGithubAction,
        TestHelper.IsWindows);

    public static DbDriver[] SelectDrivers(string? selection, bool isGithubAction, bool isWindows)
    {
        if (selection is null)
        {
            return isGithubAction
                ? isWindows
                    ? [
                        DbDriver.Npgsql,
                        DbDriver.Sqlite,
                        DbDriver.SqlServer,
                    ]
                    : [
                        DbDriver.Oracle,
                        DbDriver.MySqlConnector,
                    ]
                : [
                    DbDriver.MySqlConnector,
                    DbDriver.SqlServer,
                    DbDriver.Sqlite,
                ];
        }

        var drivers = new HashSet<DbDriver>();
        foreach (var name in selection.Split(',').Select(m => m.Trim()).Where(m => m.IsNotEmpty()))
        {
            if (Enum.TryParse<DbDriver>(name, ignoreCase: true, fromNumeric: false, out var driver) == false
                || SupportedDrivers.Contains(driver) == false)
            {
                throw new ArgumentException(
                    $"Unsupported database driver '{name}'. Supported drivers: {string.Join(", ", SupportedDrivers)}.",
                    nameof(selection));
            }

            drivers.Add(driver);
        }

        if (drivers.IsEmpty())
            throw new ArgumentException(
                $"No supported database drivers were selected. Supported drivers: {string.Join(", ", SupportedDrivers)}.",
                nameof(selection));

        return drivers.ToArray();
    }

    /// <summary>Combines selected drivers with schema scenarios, including only null-schema cases for SQLite.</summary>
    public static IEnumerable<(DbDriver Driver, string? Schema)> GetDriverSchemaCases(IEnumerable<string?> schemas)
    {
        return from driver in SelectedDrivers
               from schema in schemas
#pragma warning disable IDE0078
               where driver is not DbDriver.Sqlite || schema is null
#pragma warning restore IDE0078
               select (driver, schema);
    }

    /// <summary>Sets a PostgreSQL identity sequence's next value to one greater than the table's maximum key.</summary>
    /// <returns>The configured next sequence value for PostgreSQL, or zero for other drivers or a missing table.</returns>
    /// <remarks>
    /// Uses the shared test model's integer Id column. This changes persistent sequence state and must
    /// run without concurrent inserts into the same table.
    /// </remarks>
    public static async Task<int> SynchronizeIdentitySequenceAsync<T>(DbConnection connection, DbDriver driver, string? schema)
    {
        if (driver != DbDriver.Npgsql)
            return 0;
        if (!await connection.TableExistsAsync<T>(schema))
            return 0;

        var table = DapperHelper.GetTableNameWithSchema(connection, schema, typeof(T));
        return await connection.ExecuteScalarAsync<int>($"""
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
