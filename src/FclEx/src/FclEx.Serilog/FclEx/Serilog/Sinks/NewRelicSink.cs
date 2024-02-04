namespace FclEx.Serilog.Sinks;

public class NewRelicSink : IBatchedLogEventSink
{
    public const int DefaultBatchSizeLimit = 100;
    public const string DefaultEndpoint = "https://log-api.newrelic.com/log/v1";
    public static readonly TimeSpan DefaultPeriod = TimeSpan.FromSeconds(2);

    private readonly string _endpointUrl;
    private readonly ITextFormatter _formatter;
    private readonly HttpClient _httpClient;

    public NewRelicSink(string endpointUrl, string licenseKey, ITextFormatter formatter)
    {
        _endpointUrl = endpointUrl;
        _formatter = formatter;
        var handler = HttpClientHelper.CreateSocketsHttpHandler();

        // for fiddler
        //handler.UseProxy = true;
        //handler.Proxy = new WebProxy("http://127.0.0.1:8888");

        _httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(60),
            DefaultRequestHeaders = { { "X-License-Key", licenseKey } },
        };
    }

    public virtual async Task EmitBatchAsync(IEnumerable<LogEvent> events)
    {
        try
        {
            var body = Serialize(events, _formatter);
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, _endpointUrl)
            {
                Content = HttpContentHelper.ToGZipContent(body, HttpMediaTypes.Json)
            };

            using var response = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
            if (response.StatusCode == HttpStatusCode.Accepted)
                return;

            SelfLog.WriteLine("Self-log: Response from NewRelic Logs is missing or negative: {0}", response.StatusCode);
        }
        catch (Exception ex)
        {
            SelfLog.WriteLine("Failed to parse response from NewRelic Logs: {0} {1}", ex.Message, ex.StackTrace);
        }
    }

    public virtual Task OnEmptyBatchAsync()
    {
        return Task.CompletedTask;
    }

    protected internal static string Serialize(IEnumerable<LogEvent> events, ITextFormatter textFormatter)
    {
        using var disposable = ObjectPoolHelper.StringBuilderPool.GetAsDisposable();
        var builder = disposable.Value;
        var textWriter = new StringWriter(builder);
        textWriter.Write("[");
        foreach (var (item, _, _, isLast) in events.IndexExt())
        {
            textFormatter.Format(item, textWriter);

            if (isLast == false)
                textWriter.Write(",");
        }
        textWriter.Write("]");
        return builder.ToString();
    }
}