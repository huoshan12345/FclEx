namespace FclEx.RabbitMQ;

public sealed class StringToStringConverter : IMessageConverter<string, string>
{
    public static StringToStringConverter Instance { get; } = new();
    public Task<string> Convert(string source) => Task.FromResult(source);
}