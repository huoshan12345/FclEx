namespace FclEx.Extensions;

public static class SocketExtensions
{
#if !NET5_0_OR_GREATER
    public static async Task ConnectAsync(this Socket socket, IPAddress host, int port, CancellationToken token)
    {
        var task = Task.Factory.FromAsync(
            socket.BeginConnect,
            socket.EndConnect,
            host, port, null);

        using (token.Register(() => socket.Close()))
        {
            try
            {
                await task;
            }
            catch (ObjectDisposedException) when (token.IsCancellationRequested)
            {
                throw new OperationCanceledException(token);
            }
        }
    }
#endif
}
