namespace FclEx.RabbitMQ.Testers;

public sealed class StringToStringAsyncMsgConverter : IAsyncMsgConverter<string, string>
{
    public static StringToStringAsyncMsgConverter Instance { get; } = new();
    public Task<string> Convert(string source) => Task.FromResult(source);
}