namespace FclEx.Http.Services;

public class HttpServiceBaseTests
{
    [Fact]
    public async Task SendAsync_WhenInternalExecutionSucceeds_ReturnsResponseWithRequestAndTiming()
    {
        var service = new TestHttpService();
        var request = HttpRequest.Get("https://example.com/api");

        var response = await service.SendAsync(request);

        Assert.Same(request, response.Request);
        Assert.False(response.IsError);
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.NotEqual(default, response.StartTime);
        Assert.True(response.Elapsed >= TimeSpan.Zero);
        Assert.True(response.EndTime >= response.StartTime);
        Assert.Equal(1, service.ExecuteCallCount);
    }

    [Fact]
    public async Task SendAsync_WhenInternalExecutionThrows_ReturnsErrorResponse()
    {
        var exception = new InvalidOperationException("send failed");
        var service = new TestHttpService
        {
            Exception = exception,
        };
        var request = HttpRequest.Get("https://example.com/api");

        var response = await service.SendAsync(request);

        Assert.Same(request, response.Request);
        Assert.True(response.IsError);
        Assert.Same(exception, response.Exception);
        Assert.NotEqual(default, response.StartTime);
        Assert.True(response.Elapsed >= TimeSpan.Zero);
        Assert.Equal(1, service.ExecuteCallCount);
    }

    [Fact]
    public async Task SendAsync_WhenTokenIsAlreadyCanceled_ThrowsWithoutExecutingInternal()
    {
        var service = new TestHttpService();
        var request = HttpRequest.Get("https://example.com/api");
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.SendAsync(request, cts.Token));

        Assert.Equal(0, service.ExecuteCallCount);
    }

    [Fact]
    public void Logger_WhenSetToNull_UsesNullLogger()
    {
        var service = new TestHttpService
        {
            Logger = null,
        };

        Assert.Same(Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance, service.Logger);
    }

    private sealed class TestHttpService : HttpServiceBase
    {
        public int ExecuteCallCount { get; private set; }

        public Exception? Exception { get; init; }

        protected override Task ExecuteAsyncInternal(HttpRequest request, HttpResponse response, CancellationToken token)
        {
            ExecuteCallCount++;
            if (Exception is not null)
            {
                throw Exception;
            }

            response.StatusCode = HttpStatusCode.Accepted;
            return Task.CompletedTask;
        }
    }
}
