using System.Net.Sockets;

namespace FclEx.Serilog;

internal class TcpInput : ILogstashInput
{
    private readonly Uri _uri;
    public TcpInput(Uri uri)
    {
        _uri = uri;
    }

    private static readonly char _newLine = '\n';

    public async Task SendAsync(IEnumerable<string> list)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(_uri.Host, _uri.Port).IgnoreSyncContext();
        await using var writer = new StreamWriter(client.GetStream());
        foreach (var item in list)
        {
            await writer.WriteAsync(item);
            await writer.WriteAsync(_newLine);
        }
        //writer.Write(_newLine);
        await writer.FlushAsync().IgnoreSyncContext();
    }
}