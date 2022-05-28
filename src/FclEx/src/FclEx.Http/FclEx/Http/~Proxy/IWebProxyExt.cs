using System.Net;

namespace FclEx.Http;

public interface IWebProxyExt : IWebProxy
{
    ProxyType Type { get; }
    string? Host { get; }
    int Port { get; }
}