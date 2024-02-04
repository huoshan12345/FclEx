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
        Func<string, (SocketEndpoint, string)> createNewConnectionString) where T : DbContext
    {
        if (ssh == null)
            return new SshDbContext<T>(createContext(connectionString), null, null);

        var sshClient = new SshClient(ssh);
        sshClient.Connect();

        var (host, port) = IPEndPointHelper.NextLocalEndpoint();
        var (newEndpoint, newConnectionString) = createNewConnectionString(connectionString);
        var tunnel = new ForwardedPortLocal(host, (uint)port, newEndpoint.Host, (uint)newEndpoint.Port);
        sshClient.AddForwardedPort(tunnel);
        tunnel.Start();

        var context = createContext(newConnectionString);
        return new(context, sshClient, tunnel);
    }
}