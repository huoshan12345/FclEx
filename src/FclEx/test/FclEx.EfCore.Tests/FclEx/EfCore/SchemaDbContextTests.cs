using FclEx.Dapper;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;

namespace FclEx.EfCore;

public class SchemaDbContextTests(EfCoreFixture fixture) : EfCoreTests(fixture)
{
    private static async Task TestData(GlobalDbContext context, string? schema)
    {
        var entity = new EntityWithAutoKey
        {
            Name = Guid.NewGuid().ToString(),
            Value = 1,
        };
        context.EntityWithAutoKey.Add(entity);
        await context.SaveChangesAsync();

        Assert.NotEqual(default, entity.Id);

        var entityFromDb = await context.Database.GetDbConnection()
            .GetAsync<EntityWithAutoKey>(entity.Id, schema);

        Assert.NotNull(entityFromDb);
        Assert.Equal(entity.Name, entityFromDb.Name);
        Assert.Equal(entity.Value, entityFromDb.Value);
    }

    private async Task<string?> GetUserDefaultSchema(DbProviderType dbProviderType)
    {
        var cs = Fixture.ConnectionStrings;
        switch (dbProviderType)
        {
            case DbProviderType.Npgsql:
            {
                var conStr = cs.Get(DbProviderType.Npgsql, true);
                await using var con = new NpgsqlConnection(conStr);
                return await con.ExecuteScalarAsync<string>("SHOW SEARCH_PATH;");
            }
            case DbProviderType.SqlServer:
            {
                var conStr = cs.Get(DbProviderType.SqlServer, true);
                await using var con = new SqlConnection(conStr);
                return await con.ExecuteScalarAsync<string>("SELECT SCHEMA_NAME();");
            }
            case DbProviderType.MySql:
            case DbProviderType.MySqlConnector:
            {
                return new MySqlConnectionStringBuilder(cs.Get(DbProviderType.MySql, true)).Database;
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
            Assert.Equal(Fixture.DefaultUser.DefaultSchema, defaultSchema);
        }

        await using var context = Fixture.CreateDbContext(dbProviderType, null, true);
        await TestData(context, defaultSchema);
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task DbContext_WithSchema_Test(DbProviderType dbProviderType, string? schema)
    {
        // the default schema for user will be used.
        await using var context = Fixture.CreateDbContext(dbProviderType, schema);
        await TestData(context, Fixture.WithAssemblyInfoIfNotNull(schema));
    }
}