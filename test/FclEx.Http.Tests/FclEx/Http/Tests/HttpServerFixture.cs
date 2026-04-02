using FclEx.Tests;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using static Duende.IdentityModel.OidcConstants;

namespace FclEx.Http.Tests;

public class HttpServerFixture : GlobalFixture
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

    private static readonly Lazy<string> VisitorHtml = new(() =>
        ResourceHelper.Embedded.ReadString(typeof(HttpServerFixture).Assembly, "visitor.html"));
    private static readonly Encoding Gb2312 = Encoding.GetEncoding("gb2312");

    private static SymmetricSecurityKey GetSecurityKey()
    {
        const string key = "MTExMTExMTExMTExMTExMTExMTExMTExMTExMTExMTExMQ==";
        return new SymmetricSecurityKey(key.Base64ToBytes());
    }

    public static string CreateToken(string[] scopes)
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

    public const string TokenPath = "/oauth/openid-connect/token";
    public const string RequiredScope = "test-scope";

    private static async Task RunApiServer()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls(TestUri.ToString());
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
            var error = feature?.Error is { } ex
                ? ex.Message
                : "Unknown error occurred.";
            await context.Response.WriteAsync(error);
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
                { "headers", JsonNode.From(headers) },
            };
            await context.Response.WriteAsync(obj.ToString());
        });

        app.MapPost("/api/charset", (HttpContext context, string charSet) =>
        {
            context.Response.ContentType = $"text/plain;charset={charSet}";
        });

        app.MapGet("/api/redirect", (string u) => Results.Redirect(u));

        app.MapGet("/api/charset-detect/gb2312", async context =>
        {
            context.Response.ContentType = MediaTypes.Html; // do not set charset, to test auto-detect encoding
            await context.Response.WriteAsync(VisitorHtml.Value, Gb2312);
        });

        app.MapGet("/oauth/.well-known/openid-configuration", async context =>
        {
            var request = context.Request;
            var issuer = $"{request.Scheme}://{request.Host}/oauth";
            var token = $"{request.Scheme}://{request.Host}{TokenPath}";
            var discovery = new JsonObject
            {
                {Discovery.Issuer, issuer},
                {Discovery.TokenEndpoint, token},
            };
            await context.Response.WriteAsync(discovery.ToJsonString());
        });

        app.MapPost(TokenPath, async context =>
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

        app.MapGet("/auth/test", [Authorize, RequiredScope(RequiredScope)] async (context) =>
        {
            var auth = context.Request.Headers.Authorization.ToString();
            await context.Response.WriteAsync(auth);
        });

        await app.StartAsync();
    }


    public override async ValueTask InitializeAsync()
    {
        await RunApiServer();
    }
}