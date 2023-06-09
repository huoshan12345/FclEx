using FclEx.Http;

namespace FclEx.Web.Testing;

public readonly record struct LoginOptions(
    bool Login, 
    bool FakeLogin,
    bool UseCache, 
    bool ReadCookie, 
    IWebProxyExt Proxy);