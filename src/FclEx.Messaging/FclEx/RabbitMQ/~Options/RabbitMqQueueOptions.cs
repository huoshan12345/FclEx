namespace FclEx.RabbitMQ;

public class RabbitMqQueueOptions
{
    public string Name { get; set; } = string.Empty;
    public string[] BindKeys { get; set; } = [];
    public ushort PrefetchCount { get; set; } = 1;
}