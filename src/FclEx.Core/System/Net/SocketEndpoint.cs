namespace System.Net;

public readonly record struct SocketEndpoint(string Host, int Port)
{
    public static implicit operator DnsEndPoint(SocketEndpoint endpoint)
    {
        return new(endpoint.Host, endpoint.Port);
    }

    public static implicit operator SocketEndpoint(DnsEndPoint endpoint)
    {
        return new(endpoint.Host, endpoint.Port);
    }

    public static implicit operator SocketEndpoint((string Host, int Port) endpoint)
    {
        return new(endpoint.Host, endpoint.Port);
    }
}