using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace FclEx.Serilog.Sinks;

public class NewRelicSinkTests
{
    [Fact]
    public void Dispose_OwnedClient_DisposesHandlerOnce()
    {
        var handler = new TrackingHandler();
        using var client = new HttpClient(handler);
        var sink = new NewRelicSink(client, "test-license", disposeHttpClient: true);

        sink.Dispose();
        sink.Dispose();

        Assert.Equal(1, handler.DisposeCount);
    }

    [Fact]
    public async Task Dispose_SharedClient_PreservesClientAndPerSinkAuthentication()
    {
        var handler = new TrackingHandler();
        using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(2) };
        client.DefaultRequestHeaders.Add("X-License-Key", "caller-license");
        using var first = new NewRelicSink(client, "first-license");
        using var second = new NewRelicSink(client, "second-license");

        await first.EmitBatchAsync(new[] { CreateLogEvent(1) });
        first.Dispose();
        await second.EmitBatchAsync(new[] { CreateLogEvent(2) });

        Assert.Equal(0, handler.DisposeCount);
        Assert.Equal(new[] { "first-license", "second-license" }, handler.LicenseKeys);
        Assert.Equal("caller-license", client.DefaultRequestHeaders.GetValues("X-License-Key").Single());
        Assert.Equal(TimeSpan.FromSeconds(2), client.Timeout);
        await Assert.ThrowsAsync<ObjectDisposedException>(() => first.EmitBatchAsync(new[] { CreateLogEvent(3) }));
    }

    [Fact]
    public async Task EmitBatchAsync_DisposesRequestContent()
    {
        var handler = new TrackingHandler();
        using var client = new HttpClient(handler);
        using var sink = new NewRelicSink(client, "test-license");

        await sink.EmitBatchAsync(new[] { CreateLogEvent(1) });

        var request = Assert.Single(handler.Requests);
        await Assert.ThrowsAsync<ObjectDisposedException>(() => request.Content!.ReadAsByteArrayAsync());
    }

    [Fact]
    public void DisposeLogger_DisposesBatchedNewRelicSink()
    {
        var handler = new TrackingHandler();
        using var client = new HttpClient(handler);
        var sink = new NewRelicSink(client, "test-license", disposeHttpClient: true);
        var configuration = new SerilogConfiguration().WriteTo(writeTo => writeTo.Sink(sink, new global::Serilog.Configuration.BatchingOptions()));
        var logger = (Logger)configuration.CreateLogger();

        logger.Dispose();

        Assert.Equal(1, handler.DisposeCount);
    }

    [Fact]
    public void Constructor_NullClient_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new NewRelicSink((HttpClient)null!, "test-license"));
    }

    private sealed class TrackingHandler : HttpMessageHandler
    {
        public int DisposeCount { get; private set; }
        public List<string> LicenseKeys { get; } = [];
        public List<HttpRequestMessage> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LicenseKeys.Add(request.Headers.GetValues("X-License-Key").Single());
            Requests.Add(request);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                DisposeCount++;
            base.Dispose(disposing);
        }
    }

    [Theory]
    [InlineData(202, true)]
    [InlineData(200, false)]
    [InlineData(400, false)]
    [InlineData(429, false)]
    [InlineData(503, false)]
    public async Task EmitBatchAsync_RequiresAcceptedResponse(int statusCode, bool accepted)
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        try
        {
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            using var sink = new NewRelicSink("test-license", $"http://127.0.0.1:{port}/log/v1");
            var timeout = TimeSpan.FromSeconds(10);
            var sending = sink.EmitBatchAsync(new[] { CreateLogEvent(1) });
            using var connection = await listener.AcceptTcpClientAsync().WaitAsync(timeout);
            using var stream = connection.GetStream();
            using var reader = new StreamReader(stream, Encoding.ASCII, false, 1024, leaveOpen: true);

            string? header;
            do
            {
                header = await reader.ReadLineAsync().WaitAsync(timeout);
                Assert.NotNull(header);
            } while (header.Length > 0);

            var response = Encoding.ASCII.GetBytes($"HTTP/1.1 {statusCode} Test\r\nContent-Length: 0\r\nConnection: close\r\n\r\n");
            await stream.WriteAsync(response, 0, response.Length);

            if (accepted)
            {
                await sending.WaitAsync(timeout);
            }
            else
            {
                var exception = await Assert.ThrowsAsync<HttpRequestException>(() => sending.WaitAsync(timeout));
                Assert.Contains(statusCode.ToString(), exception.Message);
            }
        }
        finally
        {
            listener.Stop();
        }
    }

    [Fact]
    public async Task EmitBatchAsync_TransportFailure_PropagatesForBatchRetry()
    {
        using var sink = new NewRelicSink("test-license", "unsupported://localhost/log/v1");
        var events = new[] { CreateLogEvent(1) };

#if NETFRAMEWORK
        await Assert.ThrowsAsync<ArgumentException>(() => sink.EmitBatchAsync(events));
#else
        await Assert.ThrowsAsync<NotSupportedException>(() => sink.EmitBatchAsync(events));
#endif
    }

    [Fact]
    public async Task EmitBatchAsync_SerializationFailure_PropagatesForBatchRetry()
    {
        var exception = new FormatException("formatting failed");
        var formatter = new Mock<global::Serilog.Formatting.ITextFormatter>();
        formatter.Setup(m => m.Format(It.IsAny<LogEvent>(), It.IsAny<TextWriter>())).Throws(exception);
        using var sink = new NewRelicSink("test-license", formatter: formatter.Object);

        var actual = await Assert.ThrowsAsync<FormatException>(() => sink.EmitBatchAsync(new[] { CreateLogEvent(1) }));

        Assert.Same(exception, actual);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    public void Serialize_Test(int count)
    {
        var events = Enumerable.Range(1, count).Select(CreateLogEvent);
        var str = NewRelicSink.Serialize(events, new JsonFormatter());
        var token = JsonDocument.Parse(str);
        Assert.Equal(JsonValueKind.Array, token.RootElement.ValueKind);
    }

    [Fact(Explicit = true)]
    public async Task EmitBatchAsync_Test()
    {
        var writer = new StringWriter();
        using var x = writer.SetSelfLog();

        using var sink = new NewRelicSink(licenseKey: "");
        var events = Enumerable.Range(1, 5).Select(CreateLogEvent).ToArray();
        await sink.EmitBatchAsync(events);

        var str = writer.ToString();
        Assert.Empty(str);
    }

    private static LogEvent CreateLogEvent(int number)
    {
        var template = new MessageTemplate([new TextToken("message_" + number)]);
        var props = Enumerable.Range(1, 3).Select(m => new LogEventProperty("prop" + m, new ScalarValue("value" + m)));
        return new LogEvent(Random.Shared.NextDateTime(), LogEventLevel.Information, null, template, props);
    }
}
