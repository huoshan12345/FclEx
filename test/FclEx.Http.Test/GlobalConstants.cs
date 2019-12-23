using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FclEx.Http.Core.Cookies;
using FclEx.Http.Proxy;

namespace FclEx.Http.Test
{
    public static class GlobalConstants
    {
        public static string GetDefaultProxyUrl()
        {
            return Environment.MachineName switch
            {
                "JEREMYLI" => "http://jeremyli:huoshan%40123@10.32.184.8:8888",
                _ => "http://localhost:1080",
            };
        }

        public static IWebProxyExt DefaultProxy { get; } = WebProxyExt.Create(GetDefaultProxyUrl());

        public static IReadOnlyList<SimpleCookie> SimpleCookies { get; }
            = File.ReadAllText(Path.Combine("~TestData", "SimpleCookies.json"))
                .ToJToken()
                .ToObject<List<SimpleCookie>>();
    }
}
