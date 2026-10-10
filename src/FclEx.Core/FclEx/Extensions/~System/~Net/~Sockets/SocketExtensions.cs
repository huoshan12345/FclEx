namespace FclEx.Extensions;

public static class SocketExtensions
{
#if !NET5_0_OR_GREATER
    public static async Task ConnectAsync(this Socket socket, IPAddress host, int port, CancellationToken cancellationToken)
    {
        var task = Task.Factory.FromAsync(
            socket.BeginConnect,
            socket.EndConnect,
            host, port, null);

        using (cancellationToken.Register(socket.Close))
        {
            try
            {
                await task;
            }
            catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException(cancellationToken);
            }
        }
    }
#endif
}
