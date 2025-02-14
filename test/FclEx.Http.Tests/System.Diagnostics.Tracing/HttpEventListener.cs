namespace System.Diagnostics.Tracing;

internal sealed class HttpEventListener : EventListener
{
    private readonly ITestOutputHelper _output;

    public HttpEventListener(ITestOutputHelper output)
    {
        _output = output;
    }

    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        // Allow internal HTTP logging
        if (eventSource.Name is "Private.InternalDiagnostics.System.Net.Http" or "System.Net.Http")
        {
            EnableEvents(eventSource, EventLevel.LogAlways);
        }
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        var time = eventData.TimeStamp.SpecifyKind(DateTimeKind.Local);
        var sb = new StringBuilder().Append($"[{time:HH:mm:ss.ffffff}][{eventData.EventName}] ");

        foreach (var (_, (name, item), isFirst, _) in eventData.PayloadNames.EmptyIfNull().Zip(eventData.Payload.EmptyIfNull()).IndexEx())
        {
            if (isFirst == false)
                sb.Append(", ");
            sb.Append(name).Append(": ").Append(item);
        }
        _output.WriteLine(sb.ToString());
    }
}