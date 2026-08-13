using Renci.SshNet.Common;

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
        try
        {
            await Context.DisposeAsync();
        }
        finally
        {
            try
            {
                Tunnel?.Dispose();
            }
            finally
            {
                SshClient?.Dispose();
            }
        }
    }
}

public class SshDbContext
{
    /// <summary>
    /// Creates a database context that optionally connects through a local SSH tunnel.
    /// </summary>
    /// <typeparam name="T">The database context type.</typeparam>
    /// <param name="connectionString">The original database connection string.</param>
    /// <param name="createContext">Creates the database context from the effective connection string.</param>
    /// <param name="ssh">The SSH connection information, or <see langword="null"/> to connect without a tunnel.</param>
    /// <param name="createNewConnectionString">
    /// Creates the remote database endpoint and a connection string targeting the supplied local tunnel endpoint.
    /// </param>
    /// <param name="hostKeyReceived">
    /// An optional handler registered before the SSH connection is opened. The handler can inspect the server key
    /// and set <see cref="HostKeyEventArgs.CanTrust"/> to control whether the connection is accepted.
    /// </param>
    /// <returns>A wrapper that owns the context and any SSH resources created for it.</returns>
    public static SshDbContext<T> CreateSshDbContext<T>(string connectionString, Func<string, T> createContext, ConnectionInfo? ssh,
        Func<string, SocketEndpoint, (SocketEndpoint RemoteEndpoint, string ConnectionString)> createNewConnectionString,
        EventHandler<HostKeyEventArgs>? hostKeyReceived = null) where T : DbContext
    {
        if (ssh == null)
            return new SshDbContext<T>(createContext(connectionString), null, null);

        var sshClient = new SshClient(ssh);
        ForwardedPortLocal? tunnel = null;
        T? context = null;

        try
        {
            if (hostKeyReceived is not null)
                sshClient.HostKeyReceived += hostKeyReceived;

            sshClient.Connect();

            var localEndpoint = IPEndPointHelper.NextLocalEndpoint();
            var (remoteEndpoint, newConnectionString) = createNewConnectionString(connectionString, localEndpoint);
            tunnel = new ForwardedPortLocal(
                localEndpoint.Host,
                (uint)localEndpoint.Port,
                remoteEndpoint.Host,
                (uint)remoteEndpoint.Port);
            sshClient.AddForwardedPort(tunnel);
            tunnel.Start();

            context = createContext(newConnectionString);
            return new(context, sshClient, tunnel);
        }
        catch
        {
            try
            {
                context?.Dispose();
            }
            finally
            {
                try
                {
                    tunnel?.Dispose();
                }
                finally
                {
                    sshClient.Dispose();
                }
            }

            throw;
        }
    }
}
