using System.Threading;

namespace FclEx.Serilog;

/// <summary>Sends JSON log batches to New Relic Logs and manages ownership of its HTTP client.</summary>
public class NewRelicSink : IBatchedLogEventSink, IDisposable
{
    /// <summary>The extension-method batching default of 100 events per request.</summary>
    public const int DefaultBatchSizeLimit = 100;
    /// <summary>The US New Relic Logs API endpoint.</summary>
    public const string DefaultEndpoint = "https://log-api.newrelic.com/log/v1";
    /// <summary>The extension-method batching interval default of two seconds.</summary>
    public static readonly TimeSpan DefaultPeriod = TimeSpan.FromSeconds(2);

    private readonly string _endpointUrl;
    private readonly ITextFormatter _formatter;
    private readonly HttpClient _httpClient;
    private readonly string _licenseKey;
    private readonly bool _disposeHttpClient;
    private int _disposed;

    /// <summary>Creates a sink that owns an HTTP client with a 60-second request timeout.</summary>
    /// <param name="licenseKey">The license key sent with each request.</param>
    /// <param name="endpointUrl">The New Relic Logs API endpoint.</param>
    /// <param name="formatter">The event formatter, or null to use the default JSON formatter.</param>
    public NewRelicSink(string licenseKey, string endpointUrl = DefaultEndpoint, ITextFormatter? formatter = null)
        : this(new HttpClient(HttpMessageHandler.CreateSocketsHttpHandler()) { Timeout = TimeSpan.FromSeconds(60) },
            licenseKey, endpointUrl, formatter, disposeHttpClient: true)
    {
    }

    /// <summary>Creates a sink using a supplied HTTP client and its existing transport settings.</summary>
    /// <param name="httpClient">The client used to send batches.</param>
    /// <param name="licenseKey">The license key added to each request.</param>
    /// <param name="endpointUrl">The New Relic Logs API endpoint.</param>
    /// <param name="formatter">The event formatter, or null to use the default JSON formatter.</param>
    /// <param name="disposeHttpClient">True to transfer client ownership to the sink. Otherwise the caller owns the client.</param>
    /// <remarks>Request authentication is set on each message so multiple sinks can share a client.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="httpClient"/> is null.</exception>
    public NewRelicSink(HttpClient httpClient, string licenseKey, string endpointUrl = DefaultEndpoint,
        ITextFormatter? formatter = null, bool disposeHttpClient = false)
    {
        _httpClient = Check.NotNull(httpClient);
        _licenseKey = licenseKey;
        _endpointUrl = endpointUrl;
        _formatter = formatter ?? new JsonFormatter();
        _disposeHttpClient = disposeHttpClient;
    }

    /// <summary>Sends a gzip-compressed JSON batch to the configured New Relic Logs endpoint.</summary>
    /// <param name="events">The events to serialize and send in one request.</param>
    /// <returns>An operation that completes when the endpoint returns HTTP 202 Accepted.</returns>
    /// <exception cref="HttpRequestException">The request fails or the endpoint returns a status other than HTTP 202 Accepted.</exception>
    /// <exception cref="ObjectDisposedException">The sink has been disposed.</exception>
    /// <remarks>
    /// Serialization, transport, and timeout failures propagate to Serilog's batching infrastructure,
    /// which handles diagnostics and retries.
    /// </remarks>
    public virtual async Task EmitBatchAsync(IReadOnlyCollection<LogEvent> events)
    {
        if (Volatile.Read(ref _disposed) != 0)
            throw new ObjectDisposedException(nameof(NewRelicSink));

        var body = Serialize(events, _formatter);
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, _endpointUrl)
        {
            Content = HttpContent.GZip(body, MediaTypeNames.Application.Json),
        };
        requestMessage.Headers.Add("X-License-Key", _licenseKey);

        using var response = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
        if (response.StatusCode == HttpStatusCode.Accepted)
            return;

        throw new HttpRequestException($"New Relic Logs returned HTTP {(int)response.StatusCode} ({response.ReasonPhrase}). Expected HTTP 202 Accepted.");
    }

    /// <summary>Completes without making an HTTP request when batching has no events.</summary>
    /// <returns>An already-completed task.</returns>
    public virtual Task OnEmptyBatchAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>Releases the sink and disposes its HTTP client when the sink owns that client.</summary>
    /// <remarks>Disposal is idempotent. Disposing an owned client cancels its pending requests.</remarks>
    public virtual void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        if (_disposeHttpClient)
            _httpClient.Dispose();

        GC.SuppressFinalize(this);
    }

    /// <summary>Formats events as the elements of one JSON array without compressing them.</summary>
    /// <param name="events">Events enumerated in order.</param>
    /// <param name="textFormatter">Must produce a complete JSON value for each event; its output is not parsed or escaped again.</param>
    /// <returns>The array text, including any formatter whitespace; an empty sequence produces an empty array.</returns>
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
