using System.Threading;

namespace FclEx.Messaging;

public class KafkaConsumerOptions<T>
{
    public string? Name { get; set; }
    public Func<T, Task>? MessageHandler { get; set; }
    public Func<T, Exception, Task>? ErrorHandler { get; set; }
    public string? Topic { get; set; }
    public ILogger? Logger { get; set; }
    public CancellationToken CancellationToken { get; set; }
    public IDeserializer<T>? Deserializer { get; set; } = new JsonDeserializer<T>();
}
