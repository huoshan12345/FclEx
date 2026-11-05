using System.Net.Sockets;
using System.Threading;

namespace FclEx.Serilog;

internal class TcpInput : ILogstashInput
{
    private readonly Uri _uri;
    private readonly TimeSpan _sendTimeout;
    private static readonly Encoding Utf8 = new UTF8Encoding(false, true);

    public TcpInput(Uri uri, TimeSpan sendTimeout)
    {
        _uri = uri;
        _sendTimeout = sendTimeout;
    }

    private const char NewLine = '\n';

    public async Task SendAsync(IEnumerable<string> list)
    {
        using var client = new TcpClient();
        // Start before connecting so synchronous completions cannot delay the start of the deadline.
        using var timeout = new CancellationTokenSource(_sendTimeout);
        // Closing the socket also interrupts I/O on older targets without cancellable ConnectAsync.
#if NET5_0_OR_GREATER
        await
#endif
        using var abort = timeout.Token.Register(static state => ((TcpClient)state!).Dispose(), client);
        var sending = SendCoreAsync(client, list, timeout.Token);
        try
        {
            // One deadline covers connecting and the complete batch, rather than restarting for each write.
            await sending.WaitAsync(timeout.Token).ConfigureAwait(false);
        }
        catch (Exception error) when (timeout.IsCancellationRequested)
        {
            // Observe failures from that aborted operation, including on older WaitAsync implementations.
            _ = sending.ContinueWith(static task => { _ = task.Exception; }, CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            throw new TimeoutException("The Logstash TCP batch exceeded its connection and send deadline.", error);
        }
    }

    private async Task SendCoreAsync(TcpClient client, IEnumerable<string> list, CancellationToken cancellationToken)
    {
#if NET6_0_OR_GREATER
        await client.ConnectAsync(_uri.Host, _uri.Port, cancellationToken).ConfigureAwait(false);
#else
        await client.ConnectAsync(_uri.Host, _uri.Port).ConfigureAwait(false);
#endif
        var stream = client.GetStream();
        foreach (var item in list)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Write bytes directly: disposing a buffered StreamWriter after a timeout could flush again.
            var bytes = Utf8.GetBytes(item + NewLine);
            await stream.WriteAsync(bytes, 0, bytes.Length, cancellationToken).ConfigureAwait(false);
        }
        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }
}
