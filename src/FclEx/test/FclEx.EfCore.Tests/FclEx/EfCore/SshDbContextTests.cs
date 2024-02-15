namespace FclEx.EfCore;

public class SshDbContextTests
{
    public const string SshKeyPath = @"d:\Users\lijing\Documents\keys\local\id_rsa";

    private static SshDbContext<GlobalDbContext> CreateNpgsqlContext(ConnectionInfo? ssh)
    {
        var connectionString = ConnectionStrings.Get(DbProviderType.Npgsql, false);
        return SshDbContext.CreateSshDbContext(connectionString, m => new GlobalDbContext(DbProviderType.Npgsql, connectionString), ssh, m =>
        {
            var builder = new NpgsqlConnectionStringBuilder(m);
            return (new(builder.Host!, builder.Port), builder.ConnectionString);
        });
    }

    [Fact]
    public async Task Connect_WithoutSsh_Test()
    {
        var ctx = CreateNpgsqlContext(null);
        await ctx.Context.Database.OpenConnectionAsync();
        await ctx.Context.Database.CloseConnectionAsync();
    }

    [LocalOnlyFact]
    public async Task Connect_WitSsh_Test()
    {
        var info = new PrivateKeyConnectionInfo("127.0.0.1", 22, "lijing", new PrivateKeyFile(SshKeyPath));
        var ctx = CreateNpgsqlContext(info);
        await ctx.Context.Database.OpenConnectionAsync();
        await ctx.Context.Database.CloseConnectionAsync();
    }
}