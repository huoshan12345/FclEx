namespace FclEx.NewRelic;

public static class ServiceCollectionExtensions
{
    public const string HttpClientName = nameof(NewRelicClient);

    public static IServiceCollection AddNewRelicClient(this IServiceCollection services, string apiKey, string? endPoint = null, HttpClientOptions? options = null)
    {
        services.AddSingletonBy<NewRelicClient, IHttpClientFactory>(m => new(() => m.CreateClient(HttpClientName), apiKey, endPoint));
        services.AddHttpClientWithPolly(HttpClientName);
        return services;
    }
}