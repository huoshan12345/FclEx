using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FclEx;
using FclEx.Extensions;
using FclEx.Http;
using FclEx.Http.Cookies;
using FclEx.Http.Proxy;

public static class GlobalConstants
{
    public static string GetDefaultProxyUrl()
    {
        return Environment.MachineName switch
        {
            _ => "http://localhost:1080",
        };
    }

    public const string TestUrl = "https://www.fastmock.site/mock/b7b0bc89cb82e6d1ffc3dc5090d39407/fclex";
    public static IWebProxyExt DefaultProxy { get; } = WebProxyExt.Create(GetDefaultProxyUrl());

    public static IReadOnlyList<SimpleCookie> SimpleCookies { get; }
        = File.ReadAllText(Path.Combine("TestData", "SimpleCookies.json"))
            .ToJToken()
            .ToObject<List<SimpleCookie>>();
}