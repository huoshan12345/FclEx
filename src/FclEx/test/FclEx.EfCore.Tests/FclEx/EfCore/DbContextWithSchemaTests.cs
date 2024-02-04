namespace FclEx.EfCore;

public readonly record struct DatabaseUser(string Username, string Password, string DefaultSchema);

public class DbContextWithSchemaTests : IAssemblyFixture<GlobalFixture>
{
    private static async Task TestData(GlobalDbContext context)
    {
        var entity = new EntityWithAutoKey
        {
            Name = nameof(DbContextWithSchema_Test),
            Value = 1,
        };
        context.EntityWithAutoKeys.Add(entity);
        await context.SaveChangesAsync();

        Assert.NotEqual(default, entity.Id);

        var entityFromDb = await context.EntityWithAutoKeys
            .AsNoTracking()
            .Where(m => m.Id == entity.Id)
            .FirstOrDefaultAsync();

        Assert.NotNull(entityFromDb);
        Assert.Equal(entity.Name, entityFromDb.Name);
        Assert.Equal(entity.Value, entityFromDb.Value);
    }

    private static async Task<string?> GetUserDefaultSchema()
    {
        await using var con = new NpgsqlConnection(LocalUserPostgresqlConnectionString);
        return await con.ExecuteScalarAsync<string>("SHOW SEARCH_PATH;");
    }

    [Fact]
    public async Task DbContextWithSchema_Test()
    {
        var schema = await GetUserDefaultSchema();
        Assert.Equal(User.DefaultSchema, schema);

        await using var context = new GlobalDbContext(LocalUserPostgresqlConnectionString, schema);
        await TestData(context);
    }

    [Fact]
    public async Task DbContextWithoutSchema_Test()
    {
        // the default schema for user will be used.
        await using var context = new GlobalDbContext(LocalUserPostgresqlConnectionString, null);
        await TestData(context);
    }

    [Fact]
    public async Task AddRange_Test()
    {
        var list = Enumerable.Range(1, 1000).Select(m => new EntityWithAutoKey
        {
            Name = Guid.NewGuid().ToString(),
            Value = m,
        }).ToArray();

        await using var context = new GlobalDbContext(LocalPostgresqlConnectionString, User.DefaultSchema);
        context.EntityWithAutoKeys.AddRange(list);
        var count = await context.SaveChangesAsync();
        Assert.Equal(list.Length, count);
    }
}