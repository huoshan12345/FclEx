namespace FclEx.Utils;

public static class HttpClientHelper
{
    public static async Task<string> GetPublicIpAsync(HttpClient httpClient)
    {
        using var res = await httpClient.GetAsync("http://ip4only.me/api/", HttpCompletionOption.ResponseHeadersRead);
        res.EnsureSuccessStatusCode();

        var str = await res.Content.ReadAsStringAsync();
        /*
            Example output
            IPv4,192.0.2.60,v1.1,,,See http://ip6.me/docs/ for api documentation
            IPv6,2001:db8:0:0:8:800:200c:417a,v1.1,,,See http://ip6.me/docs for api documentation
        */
        var ip = str.Split(',')[1];
        return ip;
    }

    public static HttpMessageHandler CreateSocketsHttpHandler(TimeSpan? connectTimeout = null)
    {
#if NETSTANDARD2_0
        return new StandardSocketsHttpHandler
#else
        return new SocketsHttpHandler
#endif
        {
            ConnectTimeout = connectTimeout ?? TimeSpan.FromSeconds(10),
            PooledConnectionLifetime = TimeSpan.FromMinutes(2), // The connection is reestablished periodically to reflect the DNS or other network changes.
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
            MaxConnectionsPerServer = ushort.MaxValue,
            UseCookies = false,
            AllowAutoRedirect = true,
            UseProxy = false,
        };
    }
}