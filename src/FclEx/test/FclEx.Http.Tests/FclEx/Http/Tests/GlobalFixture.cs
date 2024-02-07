using FclEx.AspNetCore;

namespace FclEx.Http.Tests;

public class GlobalFixture : FclEx.Tests.GlobalFixture
{
    public static readonly Uri TestUri = ((Func<Uri>)(() =>
    {
        var (host, port) = IPEndPointHelper.NextLocalEndpoint();
        return new Uri($"http://{host}:{port}");
    }))();

    public static readonly HttpClientService TestHttp = HttpClientService.Create(m =>
    {
        m.BaseAddress = TestUri;
    });

    private static string GetDefaultProxyUrl()
    {
        return Environment.MachineName switch
        {
            "PC" => "socks5://192.168.1.12:10808",
            _ => "",
        };
    }

    public static IWebProxy DefaultProxy { get; } = WebProxyHelper.Create(GetDefaultProxyUrl());

    public static IReadOnlyList<SimpleCookie> SimpleCookies { get; }
        = File.ReadAllText(Path.Combine("TestData", "SimpleCookies.json"))
            .ToJToken()
            .ToObject<List<SimpleCookie>>()!;

    private static async Task RunApiServer()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls(TestUri.ToString());
#if NET7_0_OR_GREATER
        builder.Services.AddRequestDecompression();
#endif
        var app = builder.Build();
        app.UseMiddleware<EnableBufferingMiddleware>();
#if NET7_0_OR_GREATER
        app.UseRequestDecompression();
#endif

        app.MapPost("/api/post", async context =>
        {
            var body = await context.Request.GetRawBodyAsync();
            await context.Response.WriteAsync(body);
        });

        app.MapPost("/api/compress", async context =>
        {
            var request = context.Request;
            var body = await request.GetRawBodyAsync();
            var headers = request.Headers.ToDictionary(m => m.Key, m => m.Value.ToString());
            var obj = new JObject
            {
                { "body", body.ToJToken() },
                { "encoding", request.Headers.ContentEncoding.ToString() },
                { "headers", JToken.FromObject(headers) },
            };
            await context.Response.WriteAsync(obj.ToString());
        });

        await app.StartAsync();
    }

    public override async Task InitializeAsync()
    {
        await RunApiServer();
    }

    public override Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}