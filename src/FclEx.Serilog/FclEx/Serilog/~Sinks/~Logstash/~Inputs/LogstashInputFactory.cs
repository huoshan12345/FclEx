namespace FclEx.Serilog;

internal static class LogstashInputFactory
{
    public static ILogstashInput Create(Uri uri, TimeSpan tcpSendTimeout)
    {
        return uri.Scheme switch
        {
            "udp" => new UdpInput(uri),
            "tcp" => new TcpInput(uri, tcpSendTimeout),
            _ => throw new ArgumentException("Logstash supports only tcp:// and udp:// endpoint URIs.", nameof(uri))
        };
    }
}
