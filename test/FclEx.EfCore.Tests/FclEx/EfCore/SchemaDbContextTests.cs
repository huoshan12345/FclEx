namespace FclEx.EfCore;

public class SchemaDbContextTests(EfCoreFixture fixture) : EfCoreTests(fixture)
{
    private static async Task TestData(TestDbContext context, string? schema)
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

    private async Task<string?> GetUserDefaultSchema(DbDriver dbDriver)
    {
        var cs = Fixture.ConnectionStrings;
        switch (dbDriver)
        {
            case DbDriver.Npgsql:
            {
                await using var con = cs.Get(DbDriver.Npgsql, true).CreateDbConnection();
                return await con.ExecuteScalarAsync<string>("SHOW SEARCH_PATH;");
            }
            case DbDriver.MySql:
            case DbDriver.MySqlConnector:
            {
                await using var con = cs.Get(dbDriver, true).CreateDbConnection();
                return await con.ExecuteScalarAsync<string>("SELECT SCHEMA();");
            }
            case DbDriver.SqlServer:
            {
                await using var con = cs.Get(DbDriver.SqlServer, true).CreateDbConnection();
                return await con.ExecuteScalarAsync<string>("SELECT SCHEMA_NAME();");
            }
            case DbDriver.Sqlite:
            default:
                return null;
        }
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task DbContext_UserDefaultSchema_Test(DbDriver dbDriver)
    {
        var defaultSchema = await GetUserDefaultSchema(dbDriver);
        if (dbDriver == DbDriver.Sqlite)
        {
            Assert.Null(defaultSchema);
        }
        else if (dbDriver.IsMySql())
        {
            Assert.NotNull(defaultSchema);
            var conStr = Fixture.ConnectionStrings.Get(dbDriver, true);
            Assert.Equal(conStr.Database, defaultSchema);
        }
        else
        {
            Assert.NotNull(defaultSchema);
            Assert.Equal(Fixture.DefaultUser.DefaultSchema, defaultSchema);
        }

        await using var context = Fixture.CreateDbContext(dbDriver, null, true);
        await TestData(context, defaultSchema);
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task DbContext_WithSchema_Test(DbDriver dbDriver, string? schema)
    {
        // the default schema for user will be used.
        await using var context = Fixture.CreateDbContext(dbDriver, schema);
        await TestData(context, schema);
    }
}