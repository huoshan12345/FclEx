using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Serilog.Sinks.XUnit3;

namespace FclEx.Serilog.Sinks;

public class LogstashSinkTests
{
    [Theory]
    [InlineData("https://localhost:5050")]
    [InlineData("tpc://localhost:5050")]
    [InlineData("http://localhost:5050")]
    [InlineData("ftp://localhost:5050")]
    public void Constructor_UnsupportedScheme_Throws(string uri)
    {
        Assert.ThrowsAny<ArgumentException>(() => new LogstashSink(new LogstashSinkOptions(uri)));
    }

    [Theory]
    [InlineData("tcp://localhost:5050")]
    [InlineData("udp://localhost:5050")]
    [InlineData("TCP://localhost:5050")]
    [InlineData("UDP://localhost:65535")]
    [InlineData("tcp://[::1]:5050")]
    public void Constructor_SupportedEndpoint_Completes(string uri)
    {
        _ = new LogstashSink(new LogstashSinkOptions(uri));
    }

    [Theory]
    [InlineData("tcp://localhost")]
    [InlineData("udp://localhost:0")]
    [InlineData("tcp:localhost:5050")]
    public void Constructor_MissingHostOrValidPort_Throws(string uri)
    {
        Assert.ThrowsAny<ArgumentException>(() => new LogstashSink(new LogstashSinkOptions(uri)));
    }

    [Theory]
    [InlineData("relative/path")]
    [InlineData("tcp://localhost:not-a-port")]
    [InlineData("tcp://localhost:65536")]
    public void Constructor_MalformedUri_Throws(string uri)
    {
        Assert.Throws<UriFormatException>(() => new LogstashSink(new LogstashSinkOptions(uri)));
    }

    [Fact]
    public void Constructor_NullOptions_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new LogstashSink(null!));
    }

    [Theory]
    [InlineData("tcp")]
    [InlineData("TCP")]
    public async Task EmitBatchAsync_Tcp_SendsJson(string scheme)
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        try
        {
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            var sink = new LogstashSink(new LogstashSinkOptions($"{scheme}://127.0.0.1:{port}"));
            var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Information, null, "tcp message");
            var sending = sink.EmitBatchAsync(new[] { logEvent });
            using var connection = await listener.AcceptTcpClientAsync().WaitAsync(TimeSpan.FromSeconds(10));
            using var reader = new StreamReader(connection.GetStream(), Encoding.UTF8);
            var json = await reader.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(10));
            await sending.WaitAsync(TimeSpan.FromSeconds(10));

            Assert.NotNull(json);
            using var document = JsonDocument.Parse(json);
            Assert.Equal("tcp message", document.RootElement.GetProperty("@m").GetString());
        }
        finally
        {
            listener.Stop();
        }
    }

    [Theory]
    [InlineData("udp")]
    [InlineData("UDP")]
    public async Task EmitBatchAsync_Udp_SendsJson(string scheme)
    {
        using var listener = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        var port = ((IPEndPoint)listener.Client.LocalEndPoint!).Port;
        var sink = new LogstashSink(new LogstashSinkOptions($"{scheme}://127.0.0.1:{port}"));
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Information, null, "udp message");
        var receiving = listener.ReceiveAsync();

        await sink.EmitBatchAsync(new[] { logEvent }).WaitAsync(TimeSpan.FromSeconds(10));
        var result = await receiving.WaitAsync(TimeSpan.FromSeconds(10));

        using var document = JsonDocument.Parse(Encoding.UTF8.GetString(result.Buffer));
        Assert.Equal("udp message", document.RootElement.GetProperty("@m").GetString());
    }

    [Fact(Explicit = true)]
    public async Task EmitBatchAsync_TailscaleTcp_Completes()
    {
        var sink = new LogstashSink(new LogstashSinkOptions("tcp://yecao-hk-4c8g-200m:5050"));
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Information, null,
            "FclEx.Serilog review test {TestId}", Guid.NewGuid());

        await sink.EmitBatchAsync(new[] { logEvent }).WaitAsync(TimeSpan.FromSeconds(20));
    }

    [Fact(Explicit = true)]
    public async Task Tcp_Test()
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Verbose()
            .WriteTo.XUnit3TestOutput()
            .CreateLogger();

        var logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Verbose()
            .WriteTo.Logstash("tcp://localhost:5050")
            .CreateLogger()
            .ForContext<LogstashSinkTests>();

        for (var i = 0; i < 10; i++)
        {
            // logger.Information("test message: " + i + "\n");
            logger.Error(new SimpleException("Error"), "test message: " + i);
        }

        await Log.CloseAndFlushAsync();
        await Task.Delay(TimeSpan.FromSeconds(1));
    }
}
