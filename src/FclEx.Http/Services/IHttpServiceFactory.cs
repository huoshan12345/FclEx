using FclEx.Http.Proxy;

namespace FclEx.Http.Services
{
    public interface IHttpServiceFactory
    {
        IHttpService Create(HttpServiceType type, bool useCookie = true, IWebProxyExt proxy = null);
    }
}