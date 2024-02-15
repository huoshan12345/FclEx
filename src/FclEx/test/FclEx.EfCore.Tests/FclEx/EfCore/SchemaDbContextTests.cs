using FclEx.Dapper;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;

namespace FclEx.EfCore;

public class SchemaDbContextTests : DbContextTests
{
    private static async Task TestData(GlobalDbContext context, string? schema)
    {
        var entity = new EntityWithAutoKey
        {
            Name = Guid.NewGuid().ToString(),
            Value = 1,
        };
        context.EntityWithAutoKeys.Add(entity);
        await context.SaveChangesAsync();

        Assert.NotEqual(default, entity.Id);

        var entityFromDb = await context.Database.GetDbConnection()
            .GetAsync<EntityWithAutoKey>(entity.Id, schema);

        Assert.NotNull(entityFromDb);
        Assert.Equal(entity.Name, entityFromDb.Name);
        Assert.Equal(entity.Value, entityFromDb.Value);
    }

    private static async Task<string?> GetUserDefaultSchema(DbProviderType dbProviderType)
    {
        switch (dbProviderType)
        {
            case DbProviderType.Npgsql:
            {
                var conStr = ConnectionStrings.Get(DbProviderType.Npgsql, true);
                await using var con = new NpgsqlConnection(conStr);
                return await con.ExecuteScalarAsync<string>("SHOW SEARCH_PATH;");
            }
            case DbProviderType.SqlServer:
            {
                var conStr = ConnectionStrings.Get(DbProviderType.SqlServer, true);
                await using var con = new SqlConnection(conStr);
                return await con.ExecuteScalarAsync<string>("SELECT SCHEMA_NAME();");
            }
            case DbProviderType.MySql:
            case DbProviderType.MySqlConnector:
            {
                return new MySqlConnectionStringBuilder(ConnectionStrings.MySql.User).Database;
            }
            case DbProviderType.Sqlite:
            default:
                return null;
        }
    }

    [Theory]
    [MemberData(nameof(DbTestCases))]
    public async Task DbContext_UserDefaultSchema_Test(DbProviderType dbProviderType)
    {
        var defaultSchema = await GetUserDefaultSchema(dbProviderType);
        if (dbProviderType == DbProviderType.Sqlite)
        {
            Assert.Null(defaultSchema);
        }
        else
        {
            Assert.NotNull(defaultSchema);
            Assert.Equal(DatabaseUser.Default.DefaultSchema, defaultSchema);
        }

        await using var context = GlobalDbContext.Create(dbProviderType, null, true);
        await TestData(context, defaultSchema);
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task DbContext_WithSchema_Test(DbProviderType dbProviderType, string schema)
    {
        // the default schema for user will be used.
        await using var context = GlobalDbContext.Create(dbProviderType, schema);
        await TestData(context, schema);
    }
}