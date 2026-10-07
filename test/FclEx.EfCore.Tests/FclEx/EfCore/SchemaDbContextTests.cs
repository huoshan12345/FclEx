namespace FclEx.EfCore;

public class SchemaDbContextTests(EfCoreFixture fixture) : EfCoreTests(fixture)
{
    private static async Task TestData(TestDbContext context, string? schema)
    {
        Assert.True(await context.Database.GetDbConnection().TableExistsAsync<EntityWithAutoKey>(schema,
            cancellationToken: CancellationToken));
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
        switch (dbDriver)
        {
            case DbDriver.Npgsql:
            {
                await using var con = Fixture.ResolveTarget(DbDriver.Npgsql, login: TestLogin.DefaultSchemaUser).CreateConnection();
                return await con.ExecuteScalarAsync<string>("SHOW SEARCH_PATH;");
            }
            case DbDriver.MySql:
            case DbDriver.MySqlConnector:
            {
                await using var con = Fixture.ResolveTarget(dbDriver, login: TestLogin.DefaultSchemaUser).CreateConnection();
                return await con.ExecuteScalarAsync<string>("SELECT SCHEMA();");
            }
            case DbDriver.SqlServer:
            {
                await using var con = Fixture.ResolveTarget(DbDriver.SqlServer, login: TestLogin.DefaultSchemaUser).CreateConnection();
                return await con.ExecuteScalarAsync<string>("SELECT SCHEMA_NAME();");
            }
            case DbDriver.Oracle:
            {
                await using var con = Fixture.ResolveTarget(DbDriver.Oracle, login: TestLogin.DefaultSchemaUser).CreateConnection();
                return await con.ExecuteScalarAsync<string>("SELECT USER FROM dual;");
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
            var target = Fixture.ResolveTarget(dbDriver, login: TestLogin.DefaultSchemaUser);
            Assert.Equal(target.Database, defaultSchema);
        }
        else if (dbDriver is DbDriver.Oracle)
        {
            Assert.NotNull(defaultSchema);
            var target = Fixture.ResolveTarget(dbDriver, login: TestLogin.DefaultSchemaUser);
            Assert.Equal(target.Login.UserName, defaultSchema);
        }
        else
        {
            Assert.NotNull(defaultSchema);
            Assert.Equal(Fixture.DefaultUser.DefaultSchema, defaultSchema);
        }

        await using var context = Fixture.CreateDbContext(dbDriver, null, login: TestLogin.DefaultSchemaUser);
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
