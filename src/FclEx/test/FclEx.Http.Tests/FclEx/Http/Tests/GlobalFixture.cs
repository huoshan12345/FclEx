using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RequestDecompression;
using Microsoft.Extensions.Logging;

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
            .FromJson<List<SimpleCookie>>()!;

    private static async Task RunApiServer()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls(TestUri.ToString());
        builder.Services.AddLogging(b => b.SetMinimumLevel(LogLevel.Error));

#if NET7_0_OR_GREATER
        // Should use ZlibSteam to handle deflate decompression, which has been done since aspnet core 8.
        // https://github.com/dotnet/runtime/issues/38022
        Action<RequestDecompressionOptions> action = Environment.Version.Major == 7
            ? m => m.DecompressionProviders["deflate"] = new ZLibDecompressionProvider()
            : m => { };
        builder.Services.AddRequestDecompression(action);
#endif
        var app = builder.Build();

        app.UseExceptionHandler(m => m.Run(async context =>
        {
            var feature = context.Features.Get<IExceptionHandlerPathFeature>();

            if (feature?.Error is { } ex)
            {
                await context.Response.WriteAsync(ex.Message);
            }
            else
            {
                await context.Response.WriteAsync("Unknown error occurred.");
            }
        }));

        app.UseMiddleware<EnableBufferingMiddleware>();
#if NET7_0_OR_GREATER
        app.UseRequestDecompression();
#endif
        app.MapGet("/api/sleep", async (double seconds) =>
        {
            await TaskHelper.Delay(TimeSpan.FromSeconds(seconds));
        });

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
            var obj = new JsonObject
            {
                { "body", body.ToJsonNode() },
                { "encoding", request.Headers.ContentEncoding.ToString() },
                { "headers", headers.ToJsonNode() },
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