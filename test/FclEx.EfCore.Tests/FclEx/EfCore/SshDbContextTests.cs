using Npgsql;

namespace FclEx.EfCore;

public class SshDbContextTests(EfCoreFixture fixture) : EfCoreTests(fixture)
{
    public const string SshKeyPath = @"C:\Users\lijing\.ssh\id_rsa";

    private SshDbContext<TestDbContext> CreateNpgsqlContext(ConnectionInfo? ssh)
    {
        var connectionString = Fixture.ConnectionStrings.Get(DbDriver.Npgsql, false).Build();
        return SshDbContext.CreateSshDbContext(connectionString, m => new TestDbContext(DbDriver.Npgsql, connectionString), ssh, m =>
        {
            var builder = new NpgsqlConnectionStringBuilder(m);
            return (new(builder.Host!, builder.Port), builder.ConnectionString);
        });
    }

    [Fact]
    public async Task Connect_WithoutSsh_Test()
    {
        if (DbDrivers.Contains(DbDriver.Npgsql) == false)
            return;

        var ctx = CreateNpgsqlContext(null);
        await ctx.Context.Database.OpenConnectionAsync();
        await ctx.Context.Database.CloseConnectionAsync();
    }

    [LocalOnlyFact]
    public async Task Connect_WitSsh_Test()
    {
        // ensure key is copied to ssh server.
        var info = new PrivateKeyConnectionInfo("127.0.0.1", 22, "root", new PrivateKeyFile(SshKeyPath));
        var ctx = CreateNpgsqlContext(info);
        await ctx.Context.Database.OpenConnectionAsync();
        await ctx.Context.Database.CloseConnectionAsync();
    }
}