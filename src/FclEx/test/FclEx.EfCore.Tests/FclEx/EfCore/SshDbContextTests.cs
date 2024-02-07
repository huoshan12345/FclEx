namespace FclEx.EfCore;

public class SshDbContextTests
{
    public const string SshKeyPath = @"d:\Users\lijing\Documents\keys\local\id_rsa";

    private static SshDbContext<GlobalDbContext> CreateNpgsqlContext(string connectionString, ConnectionInfo? ssh)
    {
        return SshDbContext.CreateSshDbContext(connectionString, m => new GlobalDbContext(m), ssh, m =>
        {
            var builder = new NpgsqlConnectionStringBuilder(m);
            return (new(builder.Host!, builder.Port), builder.ConnectionString);
        });
    }

    [Fact]
    public async Task Connect_WithoutSsh_Test()
    {
        var ctx = CreateNpgsqlContext(LocalPostgresqlConnectionString, null);
        await ctx.Context.Database.OpenConnectionAsync();
        await ctx.Context.Database.CloseConnectionAsync();
    }

    [LocalOnlyFact]
    public async Task Connect_WitSsh_Test()
    {
        var info = new PrivateKeyConnectionInfo("127.0.0.1", 22, "lijing", new PrivateKeyFile(SshKeyPath));
        var ctx = CreateNpgsqlContext(LocalPostgresqlConnectionString, info);
        await ctx.Context.Database.OpenConnectionAsync();
        await ctx.Context.Database.CloseConnectionAsync();
    }
}