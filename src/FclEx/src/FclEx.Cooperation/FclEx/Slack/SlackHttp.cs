using SlackNet;
using HttpClientResolver = System.Func<System.Net.Http.HttpClient>;

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

    public async Task<T?> Execute<T>(HttpRequestMessage request, CancellationToken? cancellationToken = null)
    {
        var response = await _httpClientResolver().SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken ?? CancellationToken.None);
        response.EnsureSuccessStatusCode();

        if (response.Content.Headers.ContentType?.MediaType != "application/json")
            return default;

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);
        await using var jsonTextReader = new JsonTextReader(reader);
        return _serializer.Deserialize<T>(jsonTextReader);
    }
}