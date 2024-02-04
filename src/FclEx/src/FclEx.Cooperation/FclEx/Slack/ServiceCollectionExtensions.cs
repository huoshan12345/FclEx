using SlackNet.Extensions.DependencyInjection;

namespace FclEx.Slack;

public static class ServiceCollectionExtensions
{
    public const string HttpClientName = nameof(SlackHttp);

    /// <summary>
    /// Register <see cref="SlackHttp"/> that implements <see cref="SlackNet"/>.<see cref="IHttp"/>.
    /// </summary>
    public static IServiceCollection AddSlackHttp(this IServiceCollection services, JsonSerializerSettings? jsonSettings = null, HttpClientOptions? options = null)
    {
        services.AddHttpClientWithPolly(HttpClientName, options);
        services.AddSingletonBy<SlackHttp, IHttpClientFactory>(s => new SlackHttp(() => s.CreateClient(HttpClientName), jsonSettings));
        return services;
    }

    /// <summary>
    /// Register services for <see cref="SlackNet"/> and use <see cref="SlackHttp"/> for <see cref="SlackNet"/>.<see cref="IHttp"/>.
    /// </summary>
    public static IServiceCollection AddSlackNetWithHttp(this IServiceCollection services, Action<ServiceCollectionSlackServiceConfiguration>? configure = null,
        JsonSerializerSettings? jsonSettings = null, HttpClientOptions? options = null)
    {
        return services
            .AddSlackHttp(jsonSettings, options)
            .AddSlackNet(c =>
            {
                c.UseHttp(m => m.GetRequiredService<SlackHttp>());
                configure?.Invoke(c);
            });
    }

    /// <summary>
    /// Register services for <see cref="SlackNet.AspNetCore"/> and use <see cref="SlackHttp"/> for <see cref="SlackNet"/>.<see cref="IHttp"/>.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="jsonSettings"></param>
    /// <param name="options"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static IServiceCollection AddSlackNetAspNetCoreWithHttp(this IServiceCollection services, Action<AspNetSlackServiceConfiguration>? configure = null,
        JsonSerializerSettings? jsonSettings = null, HttpClientOptions? options = null)
    {
        return services
            .AddSlackHttp(jsonSettings, options)
            .AddSlackNet(c =>
            {
                c.UseHttp(m => m.GetRequiredService<SlackHttp>());
                configure?.Invoke(c);
            });
    }
}