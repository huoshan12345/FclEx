using System.Net.Sockets;
using System.Threading.Tasks;

namespace FclEx.Serilog.Sinks.Logstash.Inputs;

internal class UdpInput : ILogstashInput
{
    private static readonly char _newLine = '\n';
    private readonly Uri _uri;
    public UdpInput(Uri uri)
    {
        _uri = uri;
    }

    public async Task SendAsync(IReadOnlyList<string> list)
    {
        using var client = new UdpClient();
        client.Connect(_uri.Host, _uri.Port);
        foreach (var item in list)
        {
            var bytes = (item + _newLine).ToBytes();
            await client.SendAsync(bytes, bytes.Length).DonotCapture();
        }
    }
}