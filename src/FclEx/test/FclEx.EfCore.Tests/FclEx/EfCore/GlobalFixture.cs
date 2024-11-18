using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using MySql.Data.MySqlClient;

namespace FclEx.EfCore;

public readonly record struct DatabaseUser(string Username, string Password, string DefaultSchema)
{
    public static readonly DatabaseUser Default = new("user_with_schema", "123456", "user_schema");
}

public readonly record struct ConnectionStrings(DbProviderType DbProviderType, string Primary, string User)
{
    public ConnectionStrings(DbProviderType dbProviderType, string primary, DatabaseUser user)
        : this(dbProviderType, primary, CreateUserConnectionString(dbProviderType, primary, user))
    {
    }

    private static string CreateUserConnectionString(DbProviderType dbProviderType, string primary, DatabaseUser user)
    {
        return dbProviderType switch
        {
            DbProviderType.Npgsql => new NpgsqlConnectionStringBuilder(primary) { Username = user.Username, Password = user.Password }.ConnectionString,
            DbProviderType.SqlServer => new SqlConnectionStringBuilder(primary) { UserID = user.Username, Password = user.Password }.ConnectionString,
            DbProviderType.Sqlite => primary,
            DbProviderType.MySql => CreateUserConnectionStringForMysql(),
            DbProviderType.MySqlConnector => CreateUserConnectionStringForMysql(),
            _ => throw new ArgumentOutOfRangeException(nameof(dbProviderType), dbProviderType, null)
        };

        string CreateUserConnectionStringForMysql()
        {
            return new MySqlConnectionStringBuilder(primary)
            {
                UserID = user.Username,
                Password = user.Password,
                Database = user.DefaultSchema,
            }.ConnectionString;
        }
    }

    public static readonly ConnectionStrings Postgresql = new(DbProviderType.Npgsql, $"Server=localhost;Database={DatabaseName};Port=5432;User Id=postgres;Password=111111", DatabaseUser.Default);
    // NOTE: do not include 'Integrated Security=sspi;' into the sql server connection string otherwise the default schema won't work.
    public static readonly ConnectionStrings SqlServer = new(DbProviderType.SqlServer, $@"Data Source=localhost\sqlexpress;Database={DatabaseName};User Id=sa;Password=a.o7a@bj;Encrypt=false", DatabaseUser.Default);
    public static readonly ConnectionStrings MySql = new(DbProviderType.MySql, $"Server=localhost;Database={DatabaseName};Port=3306;User Id=root;Password=111111;SslMode=Required", DatabaseUser.Default);
    public static readonly ConnectionStrings Sqlite = new(DbProviderType.Sqlite, $"Data Source=./{DatabaseName}.sqlite;", DatabaseUser.Default);

    public static ConnectionStrings Get(DbProviderType dbProviderType)
    {
        return dbProviderType switch
        {
            DbProviderType.MySql => MySql,
            DbProviderType.MySqlConnector => MySql,
            DbProviderType.SqlServer => SqlServer,
            DbProviderType.Npgsql => Postgresql,
            DbProviderType.Sqlite => Sqlite,
            _ => throw new ArgumentOutOfRangeException(nameof(dbProviderType), dbProviderType, null)
        };
    }

    public static string Get(DbProviderType dbProviderType, bool isUser)
    {
        var strings = Get(dbProviderType);
        return isUser ? strings.User : strings.Primary;
    }
}

public class GlobalFixture : IAsyncLifetime
{
    public static readonly string DatabaseName = typeof(GlobalDbContext).Assembly.GetName().Name!.Replace(".", "-").ToLower();
    public static readonly string?[] Schemas = [null, "schema_test_1", "schema_test_2", DatabaseUser.Default.DefaultSchema];

    public static readonly DbProviderType[] DatabaseTypes = TestHelper.IsGithubAction
        ? [DbProviderType.Npgsql, DbProviderType.Sqlite]
        : [DbProviderType.Npgsql, DbProviderType.Sqlite, DbProviderType.MySqlConnector, DbProviderType.SqlServer];

    // InitializeAsync is called immediately after the class has been created, before it is used.
    // We use this method to initialize database only once before all tests.
    public virtual async Task InitializeAsync()
    {
        foreach (var databaseType in DatabaseTypes)
        {
            var isRecreated = false; // NOTE: we delete database only once for every database instance.
            foreach (var (_, schema, isFirst, _) in Schemas.IndexExt())
            {
                await using var context = GlobalDbContext.Create(databaseType, schema);

                if (isRecreated == false || databaseType == DbProviderType.MySqlConnector)
                {
                    await context.Database.EnsureDeletedAsync();
                    await context.Database.EnsureCreatedAsync();

                    if (isRecreated == false)
                    {
                        await CreateUser(context, DatabaseUser.Default);
                    }
                }
                isRecreated = true;

                if (isFirst || databaseType is DbProviderType.Sqlite or DbProviderType.MySqlConnector)
                    continue;

                // NOTE: when database is created, the tables with the first schema are created as well, so we skip the first schema here.
                var databaseCreator = (RelationalDatabaseCreator)context.Database.GetService<IDatabaseCreator>();
                await databaseCreator.CreateTablesAsync();
            }
        }
    }

    private static async Task CreateUser(GlobalDbContext context, DatabaseUser databaseUser)
    {
        var (user, password, schema) = databaseUser;
        string[] sqls = context.DbProviderType switch
        {
            DbProviderType.Npgsql => [
                $"DROP ROLE IF EXISTS {user}",
                $"CREATE USER {user} WITH LOGIN SUPERUSER PASSWORD '{password}'",
                $"ALTER USER {user} SET SEARCH_PATH TO {schema}"
            ],
            DbProviderType.SqlServer => [
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
                $"exec sp_addrolemember 'db_owner', {user}"
                // The value of DEFAULT_SCHEMA is ignored if the user is a member of the sysadmin fixed server role.
                // All members of the sysadmin fixed server role have a default schema of dbo.
                // so we cannot assign sysadmin to the user we are going to test its default schema
                // $"ALTER SERVER ROLE [sysadmin] ADD MEMBER [{user}]",
            ],
            DbProviderType.Sqlite => [],
            DbProviderType.MySqlConnector => [
                $"DROP USER IF EXISTS {user}",
                $"CREATE USER '{user}'@'%' IDENTIFIED BY '{password}'",
                $"GRANT ALL PRIVILEGES ON *.* TO '{user}'@'%' WITH GRANT OPTION",
            ],
            DbProviderType.MySql => [],
            _ => throw new ArgumentOutOfRangeException(nameof(context.DbProviderType), context.DbProviderType, null),
        };

        foreach (var sql in sqls)
        {
#pragma warning disable EF1002 // Risk of vulnerability to SQL injection.
            await context.Database.ExecuteSqlRawAsync(sql);
#pragma warning restore EF1002 // Risk of vulnerability to SQL injection.
        }
    }

    public virtual Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}