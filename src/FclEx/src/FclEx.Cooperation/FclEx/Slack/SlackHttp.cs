using SlackNet;

namespace FclEx.Slack;

public class SlackHttp : IHttp
{
    private readonly HttpClientResolver _httpClientResolver;
    private readonly JsonSerializer _serializer;

    public SlackHttp(HttpClientResolver httpClientResolver, JsonSerializerSettings? jsonSettings = null)
    {
        _httpClientResolver = httpClientResolver ?? throw new ArgumentNullException(nameof(httpClientResolver));

        jsonSettings ??= Default.JsonSettings().SerializerSettings;
        _serializer = JsonSerializer.Create(jsonSettings);
    }

    public async Task<T?> Execute<T>(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClientResolver().SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        if (response.Content.Headers.ContentType?.MediaType != "application/json")
            return default;
#if NET6_0_OR_GREATER
        await
#endif
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);
#if NET6_0_OR_GREATER
        await
#endif
        using var jsonTextReader = new JsonTextReader(reader);
        return _serializer.Deserialize<T>(jsonTextReader);
    }
}