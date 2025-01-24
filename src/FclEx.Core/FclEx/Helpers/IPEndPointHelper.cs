namespace FclEx.Helpers;

public static class IPEndPointHelper
{
    // The TCP stack will allocate the next free one.
    private static readonly IPEndPoint DefaultLoopbackEndpoint = new(IPAddress.Loopback, port: 0);
    public static readonly IPAddress LoopbackAddress = DefaultLoopbackEndpoint.Address;

    public static SocketEndpoint NextLocalEndpoint()
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(DefaultLoopbackEndpoint);
        var port = ((IPEndPoint)socket.LocalEndPoint!).Port;
        return new(LoopbackAddress.ToString(), port);
    }
}