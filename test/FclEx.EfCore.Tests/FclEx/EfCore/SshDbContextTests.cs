using Npgsql;
using System.Net;
using System.Net.Sockets;
using Renci.SshNet.Common;

namespace FclEx.EfCore;

public class SshDbContextTests(EfCoreFixture fixture) : EfCoreTests(fixture)
{
    private sealed class TrackingDbContext : DbContext
    {
        public bool IsDisposed { get; private set; }

        public override async ValueTask DisposeAsync()
        {
            IsDisposed = true;
            await base.DisposeAsync();
        }
    }

    public const string SshKeyPath = @"C:\Users\lijing\.ssh\id_rsa";

    private SshDbContext<TestDbContext> CreateNpgsqlContext(
        ConnectionInfo? ssh,
        EventHandler<HostKeyEventArgs>? hostKeyReceived = null)
    {
        var connectionString = Fixture.ConnectionStrings.Get(DbDriver.Npgsql, false).Build();
        return SshDbContext.CreateSshDbContext(
            connectionString,
            m => new TestDbContext(DbDriver.Npgsql, m),
            ssh,
            m =>
            {
                var builder = new NpgsqlConnectionStringBuilder(m);
                return new(builder.Host!, builder.Port);
            },
            (m, localEndpoint) =>
            {
                var builder = new NpgsqlConnectionStringBuilder(m)
                {
                    Host = localEndpoint.Host,
                    Port = localEndpoint.Port,
                };
                return builder.ConnectionString;
            },
            hostKeyReceived);
    }

    [Fact]
    public async Task DisposeAsync_DisposesContext_WhenSshIsNotConfigured()
    {
        var context = new TrackingDbContext();
        var wrapper = new SshDbContext<TrackingDbContext>(context, null, null);

        Assert.Same(context, wrapper.Context);

        await wrapper.DisposeAsync();

        Assert.True(context.IsDisposed);
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

        SshClient? callbackFailureClient = null;
        Assert.Throws<InvalidOperationException>(() => SshDbContext.CreateSshDbContext(
            Fixture.ConnectionStrings.Get(DbDriver.Npgsql, false).Build(),
            m => new TestDbContext(DbDriver.Npgsql, m),
            info,
            value =>
            {
                var builder = new NpgsqlConnectionStringBuilder(value);
                return new(builder.Host!, builder.Port);
            },
            (_, _) => throw new InvalidOperationException("Connection-string factory failed."),
            (sender, args) =>
            {
                callbackFailureClient = Assert.IsType<SshClient>(sender);
                args.CanTrust = true;
            }));
        Assert.NotNull(callbackFailureClient);
        Assert.Throws<ObjectDisposedException>(() => callbackFailureClient.IsConnected);

        SshClient? contextFailureClient = null;
        SocketEndpoint? localEndpoint = null;
        var connectionString = Fixture.ConnectionStrings.Get(DbDriver.Npgsql, false).Build();
        Assert.Throws<InvalidOperationException>(() => SshDbContext.CreateSshDbContext<TestDbContext>(
            connectionString,
            _ => throw new InvalidOperationException("Context factory failed."),
            info,
            value =>
            {
                var builder = new NpgsqlConnectionStringBuilder(value);
                return new(builder.Host!, builder.Port);
            },
            (value, local) =>
            {
                localEndpoint = local;
                return value;
            },
            (sender, args) =>
            {
                contextFailureClient = Assert.IsType<SshClient>(sender);
                args.CanTrust = true;
            }));

        Assert.NotNull(contextFailureClient);
        Assert.Throws<ObjectDisposedException>(() => contextFailureClient.IsConnected);
        Assert.NotNull(localEndpoint);
        Assert.InRange(localEndpoint.Value.Port, 1, IPEndPoint.MaxPort);
        using var tcpClient = new TcpClient();
        await Assert.ThrowsAsync<SocketException>(() =>
            tcpClient.ConnectAsync(localEndpoint.Value.Host, localEndpoint.Value.Port));
    }
}
