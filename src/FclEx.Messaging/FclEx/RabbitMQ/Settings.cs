// ReSharper disable MemberCanBeProtected.Global

namespace FclEx.RabbitMQ;

public class ExchangeSettings
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = RabbitMQConstants.DefaultExchangeType;
    public bool IsDelayed { get; set; } = true;
}

public class QueueSettings
{
    public string Name { get; set; } = string.Empty;
    public string[] BindKeys { get; set; } = [];
    public ushort PrefetchCount { get; set; } = 1;
}

public class ConnectionSettings
{
    public string Host { get; set; } = "localhost";
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public int Port { get; set; } = 5672;

    public override string ToString()
    {
        return $"amqp://{HttpUtility.UrlEncode(UserName)}:{HttpUtility.UrlEncode(Password)}@{Host}:{Port}";
    }
}

public class ProcessorSettings
{
    public ConnectionSettings Connection { get; init; }
    public ExchangeSettings Exchange { get; init; }

    public ProcessorSettings()
    {
        Connection = new ConnectionSettings();
        Exchange = new ExchangeSettings();
    }

    public ProcessorSettings(ConnectionSettings connection, ExchangeSettings exchange)
    {
        Connection = connection;
        Exchange = exchange;
    }
}

public class PublisherSettings : ProcessorSettings
{
    public PublisherSettings()
    {
    }

    public PublisherSettings(ConnectionSettings connection, ExchangeSettings exchange)
        : base(connection, exchange)
    {
    }
}

public class ConsumerSettings : PublisherSettings
{
    public QueueSettings Queue { get; init; }

    public ConsumerSettings(ConnectionSettings connection, ExchangeSettings exchange, QueueSettings queue)
        : base(connection, exchange)
    {
        Queue = queue;
    }

    public ConsumerSettings()
    {
        Queue = new QueueSettings();
    }
}

public class RouterSettings : ConsumerSettings
{
    public ExchangeSettings TargetExchange { get; init; }

    public RouterSettings(ConnectionSettings connection, ExchangeSettings exchange, QueueSettings queue, ExchangeSettings targetExchange)
        : base(connection, exchange, queue)
    {
        TargetExchange = targetExchange;
    }

    public RouterSettings()
    {
        TargetExchange = new ExchangeSettings();
    }
}