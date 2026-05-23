using System.Net.Sockets;

namespace FclEx.Serilog;

internal class TcpInput : ILogstashInput
{
    private readonly Uri _uri;
    public TcpInput(Uri uri)
    {
        _uri = uri;
    }

    private const char NewLine = '\n';

    public async Task SendAsync(IEnumerable<string> list)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(_uri.Host, _uri.Port);
#if NET6_0_OR_GREATER
        await
#endif
        using var writer = new StreamWriter(client.GetStream());
        foreach (var item in list)
        {
            await writer.WriteAsync(item);
            await writer.WriteAsync(NewLine);
        }
        //writer.Write(_newLine);
        await writer.FlushAsync();
    }
}