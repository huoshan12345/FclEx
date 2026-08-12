namespace FclEx.EfCore;

public class SshDbContext<T> : IAsyncDisposable where T : DbContext
{
    public SshDbContext(T context, SshClient? sshClient, ForwardedPortLocal? tunnel)
    {
        Context = context;
        SshClient = sshClient;
        Tunnel = tunnel;
    }

    public T Context { get; }
    protected SshClient? SshClient { get; }
    protected ForwardedPortLocal? Tunnel { get; }

    public virtual async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        await Context.DisposeAsync();
        Tunnel?.Dispose();
        SshClient?.Dispose();
    }
}

public class SshDbContext
{
    public static SshDbContext<T> CreateSshDbContext<T>(string connectionString, Func<string, T> createContext, ConnectionInfo? ssh,
        Func<string, SocketEndpoint, (SocketEndpoint RemoteEndpoint, string ConnectionString)> createNewConnectionString) where T : DbContext
    {
        if (ssh == null)
            return new SshDbContext<T>(createContext(connectionString), null, null);

        // TODO: Allow host-key validation before Connect and make partial construction exception-safe.
        var sshClient = new SshClient(ssh);
        sshClient.Connect();

        var localEndpoint = IPEndPointHelper.NextLocalEndpoint();
        var (remoteEndpoint, newConnectionString) = createNewConnectionString(connectionString, localEndpoint);
        var tunnel = new ForwardedPortLocal(
            localEndpoint.Host,
            (uint)localEndpoint.Port,
            remoteEndpoint.Host,
            (uint)remoteEndpoint.Port);
        sshClient.AddForwardedPort(tunnel);
        tunnel.Start();

        var context = createContext(newConnectionString);
        return new(context, sshClient, tunnel);
    }
}
