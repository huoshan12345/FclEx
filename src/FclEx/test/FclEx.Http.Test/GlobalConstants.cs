public static class GlobalConstants
{
    public static string GetDefaultProxyUrl()
    {
        return Environment.MachineName switch
        {
            "PC" => "socks5://192.168.1.12:10808",
            _ => "",
        };
    }

    public const string TestUrl = "https://www.fastmock.site/mock/b7b0bc89cb82e6d1ffc3dc5090d39407/fclex";
    public static IWebProxy DefaultProxy { get; } = WebProxyHelper.Create(GetDefaultProxyUrl());

    public static IReadOnlyList<SimpleCookie> SimpleCookies { get; }
        = File.ReadAllText(Path.Combine("TestData", "SimpleCookies.json"))
            .ToJToken()
            .ToObject<List<SimpleCookie>>()!;
}