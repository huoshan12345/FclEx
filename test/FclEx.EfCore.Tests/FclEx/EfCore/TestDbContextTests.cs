namespace FclEx.EfCore;

public class TestDbContextTests(EfCoreFixture fixture) : EfCoreTests(fixture)
{
    public static readonly string[] OSNames = ["windows", "linux"];
    public static readonly string[] AssemblyNames = ["efcore", "dapper"];
    public static readonly int[] DotNetVersions = [4, 8, 9, 10];
    public static readonly TheoryData<DbDriver, string, int, string> SetupDatabaseCases =
        (from db in ProvisioningDrivers
         from assembly in AssemblyNames
         from ver in DotNetVersions
         from os in OSNames
         select (db, assembly, ver, os))
        .ToTheoryData();

#if NET10_0
    /// <summary>
    /// Set up databases for all test cases.
    /// Run this only when test entities are changed.
    /// </summary>
    [Theory(Explicit = true, DisableParallelization = true)]
    [MemberData(nameof(SetupDatabaseCases))]
    public async Task SetupDatabase(DbDriver dbDriver, string assemblyName, int dotNetVersion, string os)
    {
        var defaultPassword = dbDriver is DbDriver.SqlServer
            ? SqlServerUserPassword
            : DefaultUserPassword;
        var defaultUser = new DatabaseUser(WithAssemblyInfo(DefaultUserName), defaultPassword, WithAssemblyInfo(UserSchema));
        var database = WithAssemblyInfo(DatabaseName);
        using var environment = new TestDatabaseEnvironment(EfCoreFixture.Databases, database, defaultUser);

        foreach (var (_, schema, isFirst, _) in SchemaNames.IndexEx())
        {
            var currentSchema = WithAssemblyInfo(schema);
            var connectionString = environment.Resolve(dbDriver, currentSchema).BuildConnectionString();
            await using var context = new TestDbContext(dbDriver, connectionString, currentSchema);

            if (isFirst || dbDriver.IsMySql() || dbDriver is DbDriver.Oracle)
            {
                await DropDatabase(context, database);
                await CreateDatabase(context, database);

                if (isFirst)
                {
                    if (dbDriver is DbDriver.Oracle)
                    {
                        var str = environment.Resolve(dbDriver, login: TestLogin.DefaultSchemaUser).BuildConnectionString();
                        await using var ctx = new TestDbContext(dbDriver, str);
                        await CreateDatabase(ctx, defaultUser.UserName, defaultUser.Password);
                    }
                    else
                    {
                        await CreateUser(context, defaultUser);
                    }
                }
            }

            // sqlite does not support multiple schemas, so we only create tables for the first schema, and skip the rest schemas.
            if (dbDriver is DbDriver.Sqlite)
                break;

            // NOTE: when database is created, the tables with the first schema are created as well, so we skip the first schema here.
            // MySQL does not support multiple schemas in the same database.
            if (isFirst || dbDriver.IsMySql() || dbDriver is DbDriver.Oracle)
                continue;

            // create tables for the current schema.
            var databaseCreator = (RelationalDatabaseCreator)context.Database.GetService<IDatabaseCreator>();
            await databaseCreator.CreateTablesAsync();
        }

        [return: NotNullIfNotNull(nameof(str))]
        string? WithAssemblyInfo(string? str)
        {
            return CoreTestsFixture.WithAssemblyInfo(str, assemblyName, dotNetVersion, os);
        }
    }

    private static async Task DropDatabase(TestDbContext context, string database)
    {
        if (context.DbDriver.IsMySql())
        {
            var config = EfCoreFixture.Databases.MySql;
            var target = new TestDatabaseTarget(DbDriver.MySql, config, Database: "");

            var databaseName = context.Schema ?? database;
            var sql = $"""
                       SET unique_checks = 0;
                       SET foreign_key_checks = 0;
                       SET GLOBAL innodb_stats_on_metadata = 0;
                       DROP DATABASE IF EXISTS {databaseName};
                       SET GLOBAL innodb_stats_on_metadata = 1;
                       SET foreign_key_checks = 1;
                       SET unique_checks = 1;
                       """;

            await using var con = target.CreateConnection();
            await con.ExecuteAsync(sql, cancellationToken: CancellationToken);
        }
        else
        {
            var str = context.Database.GetConnectionString();
            await context.Database.EnsureDeletedAsync(cancellationToken: CancellationToken);
        }
    }

    private static async Task CreateDatabase(TestDbContext context, string database, string? password = null)
    {
        if (context.DbDriver is DbDriver.Oracle)
        {
            var config = EfCoreFixture.Databases.Oracle;
            var target = new TestDatabaseTarget(DbDriver.Oracle, config);
            var userName = context.Schema ?? database;
            var pwd = password ?? config.Password;
            await using var con = target.CreateConnection();
            await CreateUser(context.DbDriver, con, new(userName, pwd, userName));
        }

        var str = context.Database.GetConnectionString();
        await context.Database.EnsureCreatedAsync(cancellationToken: CancellationToken);
    }

    private static Task CreateUser(TestDbContext context, DatabaseUser databaseUser)
    {
        return CreateUser(context.DbDriver, context.Database.GetDbConnection(), databaseUser);
    }

    private static async Task CreateUser(DbDriver dbDriver, IDbConnection connection, DatabaseUser databaseUser)
    {
        var (user, password, schema) = databaseUser;
        string[] sqls = dbDriver switch
        {
            DbDriver.SqlServer => [
                $"""
                 IF EXISTS (SELECT * FROM master.sys.database_principals WHERE name = '{user}') 
                 BEGIN
                    DROP LOGIN {user}
                 END
                 """,
                $"""
                 IF EXISTS (SELECT * FROM master.sys.server_principals WHERE name = '{user}') 
                 BEGIN
                    DROP LOGIN {user}
                 END
                 """,
                $"CREATE LOGIN [{user}] WITH PASSWORD = N'{password}'",
                $"CREATE USER [{user}] FOR LOGIN [{user}] WITH DEFAULT_SCHEMA = {schema}",
                $"exec sp_addrolemember 'db_owner', {user}",
                // The value of DEFAULT_SCHEMA is ignored if the user is a member of the sysadmin fixed server role.
                // All members of the sysadmin fixed server role have a default schema of dbo.
                // so we cannot assign sysadmin to the user we are going to test its default schema
                // $"ALTER SERVER ROLE [sysadmin] ADD MEMBER [{user}]",
            ],
            DbDriver.Sqlite => [],
            DbDriver.Npgsql => [
                $"DROP ROLE IF EXISTS {user}",
                $"CREATE USER {user} WITH LOGIN SUPERUSER PASSWORD '{password}'",
                $"ALTER USER {user} SET SEARCH_PATH TO {schema}"
            ],
            DbDriver.MySql or DbDriver.MySqlConnector => [
                $"DROP USER IF EXISTS {user}",
                $"CREATE USER '{user}'@'%' IDENTIFIED BY '{password}'",
                $"GRANT ALL PRIVILEGES ON *.* TO '{user}'@'%' WITH GRANT OPTION",
            ],
            DbDriver.Oracle => [
                $"CREATE USER IF NOT EXISTS \"{user}\" IDENTIFIED BY \"{password}\" QUOTA UNLIMITED ON users;",
                $"GRANT CONNECT, RESOURCE TO \"{user}\";"
            ],
            _ => throw new ArgumentOutOfRangeException(nameof(dbDriver), dbDriver, null),
        };

        foreach (var sql in sqls)
        {
            await connection.ExecuteAsync(sql, cancellationToken: CancellationToken);
        }
    }

#endif
}
