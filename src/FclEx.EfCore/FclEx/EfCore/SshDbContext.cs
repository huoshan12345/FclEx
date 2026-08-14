using Renci.SshNet.Common;

namespace FclEx.EfCore;

/// <summary>
/// Owns a database context and the optional SSH resources through which it connects.
/// </summary>
/// <typeparam name="T">The database context type.</typeparam>
/// <remarks>
/// Disposing the wrapper asynchronously disposes the context first, followed by the forwarded port and SSH client.
/// </remarks>
public class SshDbContext<T> : IAsyncDisposable where T : DbContext
{
    /// <summary>
    /// Initializes a wrapper that owns the supplied context and SSH resources.
    /// </summary>
    /// <param name="context">The database context to expose and dispose.</param>
    /// <param name="sshClient">The SSH client to dispose, or <see langword="null"/> when no tunnel is used.</param>
    /// <param name="tunnel">The forwarded port to dispose, or <see langword="null"/> when no tunnel is used.</param>
    public SshDbContext(T context, SshClient? sshClient, ForwardedPortLocal? tunnel)
    {
        Context = context;
        SshClient = sshClient;
        Tunnel = tunnel;
    }

    /// <summary>
    /// Gets the database context configured with the effective connection string.
    /// </summary>
    public T Context { get; }

    /// <summary>
    /// Gets the owned SSH client, or <see langword="null"/> when the context connects directly.
    /// </summary>
    protected SshClient? SshClient { get; }

    /// <summary>
    /// Gets the owned local forwarded port, or <see langword="null"/> when the context connects directly.
    /// </summary>
    protected ForwardedPortLocal? Tunnel { get; }

    /// <summary>
    /// Disposes the context asynchronously and then releases any tunnel and SSH client.
    /// </summary>
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

/// <summary>
/// Creates database contexts that optionally connect through an SSH local port forward.
/// </summary>
public class SshDbContext
{
    /// <summary>
    /// Creates a database context that optionally connects through a local SSH tunnel.
    /// </summary>
    /// <typeparam name="T">The database context type.</typeparam>
    /// <param name="connectionString">The original database connection string.</param>
    /// <param name="createContext">Creates the database context from the effective connection string.</param>
    /// <param name="ssh">The SSH connection information, or <see langword="null"/> to connect without a tunnel.</param>
    /// <param name="getRemoteEndpoint">Gets the database endpoint to reach from the SSH server.</param>
    /// <param name="createNewConnectionString">Creates a connection string targeting the supplied local tunnel endpoint.</param>
    /// <param name="hostKeyReceived">
    /// An optional handler registered before the SSH connection is opened. The handler can inspect the server key
    /// and set <see cref="HostKeyEventArgs.CanTrust"/> to control whether the connection is accepted.
    /// </param>
    /// <returns>A wrapper that owns the context and any SSH resources created for it.</returns>
    /// <remarks>
    /// When <paramref name="ssh"/> is <see langword="null"/>, the context is created from the original connection string
    /// and the endpoint callbacks are not invoked. If setup fails after SSH resources are created, those resources are disposed before the exception is rethrown.
    /// </remarks>
    public static SshDbContext<T> CreateSshDbContext<T>(string connectionString, Func<string, T> createContext, ConnectionInfo? ssh,
        Func<string, SocketEndpoint> getRemoteEndpoint,
        Func<string, SocketEndpoint, string> createNewConnectionString,
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

            var remoteEndpoint = getRemoteEndpoint(connectionString);
            tunnel = new ForwardedPortLocal(
                IPEndPointHelper.LoopbackAddress.ToString(),
                0,
                remoteEndpoint.Host,
                (uint)remoteEndpoint.Port);
            sshClient.AddForwardedPort(tunnel);
            tunnel.Start();

            var localEndpoint = new SocketEndpoint(tunnel.BoundHost, checked((int)tunnel.BoundPort));
            var newConnectionString = createNewConnectionString(connectionString, localEndpoint);
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
