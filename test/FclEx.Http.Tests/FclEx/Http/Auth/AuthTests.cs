namespace FclEx.Http.Auth;

public class AuthTests : HttpServerTests
{
    public static IAccessTokenProvider CreateTestTokenProvider(MutateTokenResponseHandler? handler = null)
    {
        return new ServiceCollection()
            .AddTestTokenProvider(handler)
            .BuildServiceProvider()
            .GetRequiredService<IAccessTokenProviderFactory>()
            .GetRequired(string.Empty);
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

        services.AddClientCredentialsTokenProvider(options =>
        {
            options.Authority = TestUri.WithPath("/oauth").AbsoluteUri;
            options.ClientId = "client";
            options.ClientSecret = "secret";
        });

        return services;
    }
}
