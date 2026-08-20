#if NET8_0_OR_GREATER
using Microsoft.AspNetCore.Http;
#endif

namespace FclEx.Http;

public class TestApiPaths
{
    public const string Sleep = "/api/sleep";
    public const string Post = "/api/post";
    public const string Compress = "/api/compress";
    public const string Charset = "/api/charset";
    public const string Redirect = "/api/redirect";
    public const string CharsetDetectGb2312 = "/api/charset-detect/gb2312";
    public const string Discovery = "/oauth/.well-known/openid-configuration";
    public const string Token = "/oauth/openid-connect/token";
    public const string AuthTest = "/api/auth-test";
}

public class HttpServerFixture : CoreTestsFixture
{
    public static Uri TestUri { get; private set; } = null!;
    public static HttpClientService TestHttp { get; private set; } = null!;

    private static string GetDefaultProxyUrl()
    {
        return Environment.MachineName switch
        {
            "PC" => "",
            _ => "",
        };
    }

    public static string[] TestUrls { get; } =
    [
        "http://www.gstatic.com/generate_204",
        "https://www.google.com/generate_204",
        "http://cp.cloudflare.com/generate_204",
    ];

    public static IWebProxy DefaultProxy { get; } = WebProxyHelper.Create(GetDefaultProxyUrl());

    public static readonly IReadOnlyList<SimpleCookie> SimpleCookies
        = File.ReadAllText(Path.Combine("TestData", "SimpleCookies.json"))
            .FromJson<List<SimpleCookie>>()!;

    public const string RequiredScope = "test-scope";

#if NET8_0_OR_GREATER

    public static readonly bool HasApiServer = true;
    private WebApplication? _app;

    private static readonly Lazy<string> VisitorHtml = new(() => typeof(HttpServerFixture).Assembly.ReadResourceAsString("visitor.html"));
    private static readonly Encoding Gb2312 = Encoding.GetEncoding("gb2312");

    private static SymmetricSecurityKey GetSecurityKey()
    {
        const string key = "MTExMTExMTExMTExMTExMTExMTExMTExMTExMTExMTExMQ==";
        return new SymmetricSecurityKey(key.Base64ToBytes());
    }

    private static string CreateToken(string[] scopes)
    {
        var key = GetSecurityKey();
        var claims = new Dictionary<string, object>
        {
            [JwtClaimTypes.Name] = "TestUser",
            [JwtClaimTypes.JwtId] = Guid.NewGuid(),
            [JwtClaimTypes.SessionId] = Guid.NewGuid(),
            [JwtClaimTypes.Scope] = scopes.JoinWith(" "),
        };
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            Claims = claims,
            IssuedAt = DateTime.UtcNow,
            NotBefore = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddHours(1),
            IncludeKeyIdInHeader = true,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
        };

        var handler = new JsonWebTokenHandler { SetDefaultTimesOnTokenCreation = false };
        return handler.CreateToken(descriptor);
    }

    private async Task RunApiServer()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddLogging(b => b.SetMinimumLevel(LogLevel.Error));

        builder.Services
            .AddAuthentication()
            .AddJwtBearer(o =>
            {
                o.MapInboundClaims = false;
                o.RequireHttpsMetadata = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = GetSecurityKey(),
                };
            });

        builder.Services.AddSingleton<IAuthorizationHandler, ScopeAuthorizationHandler>()
            .AddAuthorizationBuilder()
            .AddDefaultPolicy("Scope", x => x
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                .AddRequirements(ScopeRequirement.Instance));

        // Should use ZlibSteam to handle deflate decompression, which has been done since aspnet core 8.
        // https://github.com/dotnet/runtime/issues/38022
        Action<RequestDecompressionOptions> action = Environment.Version.Major == 7
            ? m => m.DecompressionProviders["deflate"] = new ZLibDecompressionProvider()
            : m => { };
        builder.Services.AddRequestDecompression(action);

        var app = builder.Build();

        app.UseExceptionHandler(m => m.Run(async context =>
        {
            var feature = context.Features.Get<IExceptionHandlerPathFeature>();
            var error = feature?.Error is { } ex
                ? ex.Message
                : "Unknown error occurred.";
            await context.Response.WriteAsync(error);
        }));

        app.UseMiddleware<EnableBufferingMiddleware>();
        app.UseRequestDecompression();

        app.MapGet(TestApiPaths.Sleep, async (HttpContext context, double seconds) =>
        {
            await Task.DelaySafely(TimeSpan.FromSeconds(seconds), context.RequestAborted);
        });

        app.MapPost(TestApiPaths.Post, async context =>
        {
            var body = await context.Request.GetRawBodyAsync();
            await context.Response.WriteAsync(body);
        });

        app.MapPost(TestApiPaths.Compress, async context =>
        {
            var request = context.Request;
            var body = await request.GetRawBodyAsync();
            var headers = request.Headers.ToDictionary(m => m.Key, m => m.Value.ToString());
            var obj = new JsonObject
            {
                { "body", body.ToJsonNode() },
                { "encoding", request.Headers.ContentEncoding.ToString() },
                { "headers", JsonNode.From(headers) },
            };
            await context.Response.WriteAsync(obj.ToString());
        });

        app.MapPost(TestApiPaths.Charset, (HttpContext context, string charSet) =>
        {
            context.Response.ContentType = $"text/plain;charset={charSet}";
        });

        app.MapGet(TestApiPaths.Redirect, (string u) => Results.Redirect(u));

        app.MapGet(TestApiPaths.CharsetDetectGb2312, async context =>
        {
            context.Response.ContentType = MediaTypes.Html; // do not set charset, to test auto-detect encoding
            await context.Response.WriteAsync(VisitorHtml.Value, Gb2312);
        });

        app.MapGet(TestApiPaths.Discovery, async context =>
        {
            var request = context.Request;
            var issuer = $"{request.Scheme}://{request.Host}/oauth";
            var token = $"{request.Scheme}://{request.Host}{TestApiPaths.Token}";
            var discovery = new JsonObject
            {
                {Discovery.Issuer, issuer},
                {Discovery.TokenEndpoint, token},
            };
            await context.Response.WriteAsync(discovery.ToJsonString());
        });

        app.MapPost(TestApiPaths.Token, async context =>
        {
            var scopes = context.Request.Form[TokenRequest.Scope].ToString().Split(' ');
            var token = CreateToken(scopes);
            var tokenResponse = new JsonObject
            {
                {TokenResponse.AccessToken, token},
                {TokenResponse.ExpiresIn, 3600},
                {TokenResponse.TokenType, TokenResponse.BearerTokenType},
            };
            await context.Response.WriteAsync(tokenResponse.ToJsonString());
        });

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGet(TestApiPaths.AuthTest, [Authorize, RequiredScope(RequiredScope)] async (context) =>
        {
            var auth = context.Request.Headers.Authorization.ToString();
            await context.Response.WriteAsync(auth);
        });

        await app.StartAsync();
        var server = app.Services.GetRequiredService<Microsoft.AspNetCore.Hosting.Server.IServer>();
        var addresses = server.Features
            .Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>()
            ?.Addresses;
        var address = addresses?.SingleOrDefault()
            ?? throw new InvalidOperationException("Kestrel did not report its bound test address.");

        TestUri = new Uri(address);
        TestHttp = HttpClientService.Create(options => options.BaseAddress = TestUri);
        _app = app;
    }
#else
    public static readonly bool HasApiServer = false;

    private Task RunApiServer() => Task.CompletedTask;
#endif

    public override async ValueTask InitializeAsync()
    {
        await RunApiServer();
    }

    public override async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        await base.DisposeAsync();

#if NET8_0_OR_GREATER
        TestHttp.Dispose();
        if (_app is not null)
            await _app.DisposeAsync();
#endif
    }
}
