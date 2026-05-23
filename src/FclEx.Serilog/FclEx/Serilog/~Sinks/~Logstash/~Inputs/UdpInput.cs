using System.Net.Sockets;

namespace FclEx.Serilog;

internal class UdpInput : ILogstashInput
{
    private const char NewLine = '\n';

    private readonly Uri _uri;
    public UdpInput(Uri uri)
    {
        _uri = uri;
    }

    public async Task SendAsync(IEnumerable<string> list)
    {
        using var client = new UdpClient();
        client.Connect(_uri.Host, _uri.Port);

        foreach (var item in list)
        {
            var bytes = (item + NewLine).ToBytes();
            await client.SendAsync(bytes, bytes.Length);
        }
    }
}