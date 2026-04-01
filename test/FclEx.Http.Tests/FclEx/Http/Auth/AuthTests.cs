namespace FclEx.Http.Auth;

public class AuthTests : HttpServerTests
{
    public static IAccessTokenProvider CreateTestTokenProvider(MutateTokenResponseHandler? handler = null)
    {
        return new ServiceCollection()
            .AddTestTokenProvider()
            .BuildServiceProvider()
            .GetRequiredService<IAccessTokenProvider>();
    }
}

public static class AuthTestsExtensions
{
    public static IServiceCollection AddTestTokenProvider(this IServiceCollection services, MutateTokenResponseHandler? handler = null)
    {
        handler ??= new MutateTokenResponseHandler();
        services
            .AddHttpClient(nameof(ClientCredentialsTokenProvider))
            .AddHttpMessageHandler(m => handler);

        services.AddSingletonBy<IAccessTokenProvider, IHttpClientFactory>(m => new ClientCredentialsTokenProvider(m, new()
        {
            Authority = TestUri.WithPath("/oauth").AbsoluteUri,
            ClientId = "client",
            ClientSecret = "secret",
        }));

        return services;
    }
}
