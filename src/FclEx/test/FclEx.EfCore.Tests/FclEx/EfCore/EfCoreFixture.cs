using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using FclEx.Tests;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace FclEx.EfCore;

public class EfCoreFixture : GlobalFixture
{
    public static readonly string DatabaseName = "db".WithAssemblyInfo();
    public static readonly string?[] Schemas =
    [
        null,
        "schema_1".WithAssemblyInfo(),
        "schema_2".WithAssemblyInfo(),
        DatabaseUser.Default.DefaultSchema,
    ];

    public static readonly DbProviderType[] DatabaseTypes = TestHelper.IsGithubAction
        ? [DbProviderType.Npgsql, DbProviderType.Sqlite]
        : [DbProviderType.Npgsql, DbProviderType.Sqlite, DbProviderType.MySqlConnector, DbProviderType.SqlServer];

    // InitializeAsync is called immediately after the class has been created, before it is used.
    // We use this method to initialize database only once before all tests.
    public override async Task InitializeAsync()
    {
        Console.WriteLine("Current assembly info: " + "".WithAssemblyInfo());

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

    [ModuleInitializer]
    internal static void Initialize()
    {
        CurrentAssembly = typeof(EfCoreFixture).Assembly;
    }
}