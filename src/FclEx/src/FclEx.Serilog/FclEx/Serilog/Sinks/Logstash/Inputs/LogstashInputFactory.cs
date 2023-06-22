namespace FclEx.Serilog.Sinks.Logstash.Inputs;

internal class LogstashInputFactory
{
    public static ILogstashInput Create(LogstashInputType type, Uri uri)
    {
        switch (type)
        {
            case LogstashInputType.Udp: return new UdpInput(uri);
            case LogstashInputType.Tcp: return new TcpInput(uri);
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}