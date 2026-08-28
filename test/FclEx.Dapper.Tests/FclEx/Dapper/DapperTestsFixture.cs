using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Npgsql;

namespace FclEx.Dapper;

public class DapperTestsFixture : CoreTestsFixture
{
    public static readonly DbDriver[] DbDrivers = GetDbProviderTypes();
    public static readonly DatabasesConfig Databases = Config.GetSection("Databases").Get<DatabasesConfig>()!;

    public readonly DatabaseUser DefaultUser;
    public readonly ConnectionStrings ConnectionStrings;

    public const string DbName = "test";
    public const string UserName = "user";
    public const string UserPassword = "123456";
    public const string UserSchema = "schema";

    public DapperTestsFixture()
    {
        DefaultUser = new(WithAssemblyInfo(UserName), UserPassword, WithAssemblyInfo(UserSchema));
        ConnectionStrings = new(Databases, WithAssemblyInfo(DbName), DefaultUser);
    }

    public static readonly string?[] SchemaNames =
    [
        null,
        "schema",
        //"schema_1",
        //"schema_2",
    ];

    internal static Assembly Assembly => typeof(DapperTestsFixture).Assembly;
    internal static readonly string?[] Schemas = SchemaNames.Select(m => WithAssemblyInfo(m, Assembly)).ToArray();

    private static DbDriver[] GetDbProviderTypes()
    {
        return TestHelper.IsGithubAction
            ? TestHelper.IsWindows
                ? [DbDriver.MySqlConnector,]
                : [DbDriver.Npgsql,]
            : [
                DbDriver.MySql,
                DbDriver.MySqlConnector,
                DbDriver.Npgsql,
                DbDriver.SqlServer,
            ];
    }

    public static DbParameter CreateParameter(DbDriver dbDriver, string name, object value)
    {
        return dbDriver switch
        {
            DbDriver.SqlServer => new SqlParameter(name, value),
            DbDriver.Sqlite => new SqliteParameter(name, value),
            DbDriver.Npgsql => new NpgsqlParameter(name, value),
            DbDriver.MySql => new MySqlParameter(name, value),
            DbDriver.MySqlConnector => new MySqlConnector.MySqlParameter(name, value),
            _ => throw new ArgumentOutOfRangeException(nameof(dbDriver), dbDriver, null)
        };
    }

    public DbConnection CreateDbConnection(DbDriver dbDriver, string? schema, bool isUser = false)
    {
        var database = dbDriver.IsMySql() ? schema : null;
        return ConnectionStrings.Get(dbDriver, database, isUser).CreateDbConnection();
    }

    public virtual string?[] CurrentSchemas => Schemas;

    public override async ValueTask InitializeAsync()
    {
        foreach (var (dbDriver, schema) in DbDrivers.CrossJoin(CurrentSchemas))
        {
            using var con = CreateDbConnection(dbDriver, schema);
            await FixAutoIncrement<EntityWithAutoKey>(con, dbDriver, schema);
        }
    }

    public static Task<int> FixAutoIncrement<T>(IDbConnection con, DbDriver dbDriver, string? schema)
    {
        if (dbDriver is not DbDriver.Npgsql)
            return Task.FromResult(0);

        var tableName = DapperHelper.GetTableNameWithSchema(con, schema, typeof(T));
        var sql = $"""
                  SELECT setval(
                      pg_get_serial_sequence('{tableName}', 'Id'),
                      COALESCE((SELECT MAX("Id") FROM {tableName}), 0) + 1,
                      false
                  );
                  """;
        return con.ExecuteScalarAsync<int>(sql);
    }
}
