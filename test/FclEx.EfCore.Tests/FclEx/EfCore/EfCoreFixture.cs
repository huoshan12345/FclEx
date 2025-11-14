using FclEx.Tests;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace FclEx.EfCore;

public class EfCoreFixture : GlobalFixture
{
    public readonly DatabaseUser DefaultUser;
    public readonly ConnectionStrings ConnectionStrings;

    public EfCoreFixture()
    {
        DefaultUser = new(WithAssemblyInfo("user"), "123456", WithAssemblyInfo("schema"));
        ConnectionStrings = new(WithAssemblyInfo("db"), DefaultUser);
    }

    public static readonly string?[] Schemas =
    [
        null,
        "schema",
        "schema_1",
        "schema_2",
    ];

    public static DatabaseConfig Postgres { get; } = Config.GetSection("Postgres").Get<DatabaseConfig>()!;

    public static readonly DbProviderType[] DatabaseTypes = TestHelper.IsGithubAction
        ? [DbProviderType.Npgsql, DbProviderType.Sqlite]
        : [DbProviderType.Npgsql, DbProviderType.Sqlite, DbProviderType.MySqlConnector, DbProviderType.SqlServer];

    public GlobalDbContext CreateDbContext(DbProviderType dbProviderType, string? schema = null, bool isUser = false)
    {
        var con = ConnectionStrings.Get(dbProviderType, isUser);
        return new(dbProviderType, con, WithAssemblyInfoIfNotNull(schema));
    }

    // InitializeAsync is called immediately after the class has been created, before it is used.
    // We use this method to initialize database only once before all tests.
    public override async Task InitializeAsync()
    {
        foreach (var databaseType in DatabaseTypes)
        {
            var isRecreated = false; // NOTE: we delete database only once for every database instance.
            foreach (var (_, schema, isFirst, _) in Schemas.IndexEx())
            {
                await using var context = CreateDbContext(databaseType, schema);

                if (isRecreated == false || databaseType == DbProviderType.MySqlConnector)
                {
                    await context.Database.EnsureDeletedAsync();
                    await context.Database.EnsureCreatedAsync();

                    if (isRecreated == false)
                    {
                        await CreateUser(context, DefaultUser);
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

}