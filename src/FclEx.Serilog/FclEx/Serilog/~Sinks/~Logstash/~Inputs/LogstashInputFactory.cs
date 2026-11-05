namespace FclEx.Serilog;

internal static class LogstashInputFactory
{
    public static ILogstashInput Create(Uri uri, TimeSpan tcpSendTimeout)
    {
        switch (uri.Scheme)
        {
            case "udp": return new UdpInput(uri);
            case "tcp": return new TcpInput(uri, tcpSendTimeout);
            default:
                throw new ArgumentException("Logstash supports only tcp:// and udp:// endpoint URIs.", nameof(uri));
        }
    }
}
