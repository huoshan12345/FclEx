namespace FclEx.Serilog;

public record SinkOptions(SinkType Sink, FormatType Format)
{
    public static readonly SinkOptions ConsoleText = new(SinkType.Console, FormatType.Text);
    public static readonly SinkOptions ConsoleJson = new(SinkType.Console, FormatType.Json);
    public static readonly SinkOptions NewRelicJson = new(SinkType.NewRelic, FormatType.Json);
}