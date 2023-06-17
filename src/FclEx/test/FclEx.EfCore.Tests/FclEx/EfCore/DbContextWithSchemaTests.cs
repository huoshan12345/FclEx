using Dapper;

namespace FclEx.EfCore;

public class DbContextWithSchemaTests : IAsyncLifetime
{
    public readonly record struct DatabaseUser(string Username, string Password, string DefaultSchema);

    public static readonly DatabaseUser User = new("userwithschema", "123456", "test_schema");
    public const string LocalPostgresqlConnectionString = "Server=localhost;Database=test-efcore;Port=5432;User Id=postgres;Password=111111";
    public static readonly string LocalUserPostgresqlConnectionString = $"Server=localhost;Database=test-efcore;Port=5432;User Id={User.Username};Password={User.Password}";

    private static async Task CreateUser(DatabaseUser user)
    {
        await using var con = new NpgsqlConnection(LocalPostgresqlConnectionString);
        await con.ExecuteAsync($"DROP ROLE IF EXISTS {user.Username}");
        await con.ExecuteAsync($"CREATE USER {user.Username} WITH LOGIN SUPERUSER PASSWORD '{user.Password}'"); // so we don't need to assign permissions
        await con.ExecuteAsync($"ALTER USER {user.Username} SET SEARCH_PATH TO {user.DefaultSchema}");
    }

    private static async Task<string> GetUserDefaultSchema()
    {
        await using var con = new NpgsqlConnection(LocalUserPostgresqlConnectionString);
        return await con.ExecuteScalarAsync<string>("SHOW SEARCH_PATH;");
    }

    public async Task InitializeAsync()
    {
        await using var context = new GlobalDbContext(LocalPostgresqlConnectionString, User.DefaultSchema);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        await CreateUser(User);
    }

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

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}