namespace FclEx.EfCore;

public class GlobalFixture : IAsyncLifetime
{
    public static readonly DatabaseUser User = new("user_with_schema", "123456", "test_schema");
    public static readonly string DatabaseName = typeof(GlobalDbContext).Assembly.GetName().Name!.Replace(".", "-").ToLower();
    public static readonly string LocalPostgresqlConnectionString = $"Server=localhost;Database={DatabaseName};Port=5432;User Id=postgres;Password=111111";
    public static readonly string LocalUserPostgresqlConnectionString = $"Server=localhost;Database={DatabaseName};Port=5432;User Id={User.Username};Password={User.Password}";

    private static async Task CreateUser(DatabaseUser user)
    {
        await using var con = new NpgsqlConnection(LocalPostgresqlConnectionString);
        await con.ExecuteAsync($"DROP ROLE IF EXISTS {user.Username}");
        await con.ExecuteAsync($"CREATE USER {user.Username} WITH LOGIN SUPERUSER PASSWORD '{user.Password}'"); // so we don't need to assign permissions
        await con.ExecuteAsync($"ALTER USER {user.Username} SET SEARCH_PATH TO {user.DefaultSchema}");
    }

    public async Task InitializeAsync()
    {
        await using var context = new GlobalDbContext(LocalPostgresqlConnectionString, User.DefaultSchema);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        await CreateUser(User);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}