using System.Net;

namespace FclEx.Http.Proxy
{
    public interface IWebProxyExt : IWebProxy
    {
        ProxyType Type { get; }
        string Host { get; }
        int Port { get; }
    }
}
