using Npgsql;
using System.Net;
using Renci.SshNet.Common;

namespace FclEx.EfCore;

public class SshDbContextTests(EfCoreFixture fixture) : EfCoreTests(fixture)
{
    public const string SshKeyPath = @"C:\Users\lijing\.ssh\id_rsa";

    private SshDbContext<TestDbContext> CreateNpgsqlContext(
        ConnectionInfo? ssh,
        EventHandler<HostKeyEventArgs>? hostKeyReceived = null)
    {
        var connectionString = Fixture.ConnectionStrings.Get(DbDriver.Npgsql, false).Build();
        return SshDbContext.CreateSshDbContext(connectionString, m => new TestDbContext(DbDriver.Npgsql, m), ssh, (m, localEndpoint) =>
        {
            var builder = new NpgsqlConnectionStringBuilder(m);
            var remoteEndpoint = new SocketEndpoint(builder.Host!, builder.Port);
            builder.Host = localEndpoint.Host;
            builder.Port = localEndpoint.Port;
            return (remoteEndpoint, builder.ConnectionString);
        }, hostKeyReceived);
    }

    [Fact]
    public async Task Connect_WithoutSsh_Test()
    {
        if (DbDrivers.Contains(DbDriver.Npgsql) == false)
            return;

        await using var ctx = CreateNpgsqlContext(null, (_, _) =>
            Assert.Fail("The host-key handler must not run when SSH is disabled."));
        Assert.Equal(Fixture.ConnectionStrings.Get(DbDriver.Npgsql, false).Build(), ctx.Context.ConnectionString);
        await ctx.Context.Database.OpenConnectionAsync();
        await ctx.Context.Database.CloseConnectionAsync();
    }

    [LocalOnlyFact]
    public async Task Connect_WithSsh_Test()
    {
        // ensure key is copied to ssh server.
        var info = new PrivateKeyConnectionInfo("127.0.0.1", 22, "root", new PrivateKeyFile(SshKeyPath));
        var hostKeyReceived = false;
        await using (var ctx = CreateNpgsqlContext(info, (_, args) =>
        {
            hostKeyReceived = true;
            Assert.NotEmpty(args.FingerPrint);
            args.CanTrust = true;
        }))
        {
            var original = new NpgsqlConnectionStringBuilder(Fixture.ConnectionStrings.Get(DbDriver.Npgsql, false).Build());
            var forwarded = new NpgsqlConnectionStringBuilder(ctx.Context.ConnectionString);
            Assert.True(hostKeyReceived);
            Assert.Equal(IPAddress.Loopback.ToString(), forwarded.Host);
            Assert.NotEqual(original.Port, forwarded.Port);
            await ctx.Context.Database.OpenConnectionAsync();
            await ctx.Context.Database.CloseConnectionAsync();
        }

        var untrustedHostKeyReceived = false;

        Assert.Throws<SshConnectionException>(() => CreateNpgsqlContext(info, (_, args) =>
        {
            untrustedHostKeyReceived = true;
            args.CanTrust = false;
        }));

        Assert.True(untrustedHostKeyReceived);
    }
}
