using System;
using System.Collections.Generic;
using System.Text;
using FclEx.Http.Proxy;

namespace FclEx.Http.Test
{
    public static class GlobalConstants
    {
        public static IWebProxyExt DefaultProxy { get; } = WebProxyExt.Create("http://jeremyLi:huoshan%4012345@192.168.1.221:8888");
    }
}
