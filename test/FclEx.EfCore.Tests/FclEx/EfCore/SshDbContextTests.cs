using Npgsql;
using System.Net;

namespace FclEx.EfCore;

public class SshDbContextTests(EfCoreFixture fixture) : EfCoreTests(fixture)
{
    public const string SshKeyPath = @"C:\Users\lijing\.ssh\id_rsa";

    private SshDbContext<TestDbContext> CreateNpgsqlContext(ConnectionInfo? ssh)
    {
        var connectionString = Fixture.ConnectionStrings.Get(DbDriver.Npgsql, false).Build();
        return SshDbContext.CreateSshDbContext(connectionString, m => new TestDbContext(DbDriver.Npgsql, m), ssh, (m, localEndpoint) =>
        {
            var builder = new NpgsqlConnectionStringBuilder(m);
            var remoteEndpoint = new SocketEndpoint(builder.Host!, builder.Port);
            builder.Host = localEndpoint.Host;
            builder.Port = localEndpoint.Port;
            return (remoteEndpoint, builder.ConnectionString);
        });
    }

    [Fact]
    public async Task Connect_WithoutSsh_Test()
    {
        if (DbDrivers.Contains(DbDriver.Npgsql) == false)
            return;

        await using var ctx = CreateNpgsqlContext(null);
        Assert.Equal(Fixture.ConnectionStrings.Get(DbDriver.Npgsql, false).Build(), ctx.Context.ConnectionString);
        await ctx.Context.Database.OpenConnectionAsync();
        await ctx.Context.Database.CloseConnectionAsync();
    }

    [LocalOnlyFact]
    public async Task Connect_WithSsh_Test()
    {
        // ensure key is copied to ssh server.
        var info = new PrivateKeyConnectionInfo("127.0.0.1", 22, "root", new PrivateKeyFile(SshKeyPath));
        await using var ctx = CreateNpgsqlContext(info);
        var original = new NpgsqlConnectionStringBuilder(Fixture.ConnectionStrings.Get(DbDriver.Npgsql, false).Build());
        var forwarded = new NpgsqlConnectionStringBuilder(ctx.Context.ConnectionString);
        Assert.Equal(IPAddress.Loopback.ToString(), forwarded.Host);
        Assert.NotEqual(original.Port, forwarded.Port);
        await ctx.Context.Database.OpenConnectionAsync();
        await ctx.Context.Database.CloseConnectionAsync();
    }
}
