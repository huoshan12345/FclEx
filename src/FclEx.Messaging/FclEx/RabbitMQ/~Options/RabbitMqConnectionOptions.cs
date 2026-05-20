namespace FclEx.RabbitMQ;

public class RabbitMqConnectionOptions
{
    public string Host { get; set; } = "localhost";
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public int Port { get; set; } = 5672;

    public override string ToString()
    {
        return $"amqp://{UserName.UrlEncode()}:{Password.UrlEncode()}@{Host}:{Port}";
    }
}