namespace FclEx.Serilog;

public class NewRelicSink : IBatchedLogEventSink
{
    public const int DefaultBatchSizeLimit = 100;
    public const string DefaultEndpoint = "https://log-api.newrelic.com/log/v1";
    public static readonly TimeSpan DefaultPeriod = TimeSpan.FromSeconds(2);

    private readonly string _endpointUrl;
    private readonly ITextFormatter _formatter;
    private readonly HttpClient _httpClient;

    public NewRelicSink(string licenseKey, string endpointUrl = DefaultEndpoint, ITextFormatter? formatter = null)
    {
        _endpointUrl = endpointUrl;
        _formatter = formatter ?? new JsonFormatter();
        var handler = HttpMessageHandler.CreateSocketsHttpHandler();

        // for fiddler
        //handler.UseProxy = true;
        //handler.Proxy = new WebProxy("http://127.0.0.1:8888");

        _httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(60),
            DefaultRequestHeaders = { { "X-License-Key", licenseKey } },
        };
    }

    /// <summary>Sends a gzip-compressed JSON batch to the configured New Relic Logs endpoint.</summary>
    /// <param name="events">The events to serialize and send in one request.</param>
    /// <returns>An operation that completes when the endpoint returns HTTP 202 Accepted.</returns>
    /// <exception cref="HttpRequestException">The request fails or the endpoint returns a status other than HTTP 202 Accepted.</exception>
    /// <remarks>
    /// Serialization, transport, and timeout failures propagate to Serilog's batching infrastructure,
    /// which handles diagnostics and retries.
    /// </remarks>
    public virtual async Task EmitBatchAsync(IReadOnlyCollection<LogEvent> events)
    {
        var body = Serialize(events, _formatter);
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, _endpointUrl)
        {
            Content = HttpContent.GZip(body, MediaTypeNames.Application.Json),
        };

        using var response = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
        if (response.StatusCode == HttpStatusCode.Accepted)
            return;

        throw new HttpRequestException($"New Relic Logs returned HTTP {(int)response.StatusCode} ({response.ReasonPhrase}). Expected HTTP 202 Accepted.");
    }

    public virtual Task OnEmptyBatchAsync()
    {
        return Task.CompletedTask;
    }

    protected internal static string Serialize(IEnumerable<LogEvent> events, ITextFormatter textFormatter)
    {
        using var disposable = StringBuilder.GetCached();
        var textWriter = new StringWriter(disposable.Value);
        textWriter.Write("[");
        foreach (var (_, item, _, isLast) in events.IndexEx())
        {
            textFormatter.Format(item, textWriter);

            if (isLast == false)
                textWriter.Write(",");
        }
        textWriter.Write("]");
        return textWriter.ToString();
    }
}
