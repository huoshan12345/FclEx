using System.Linq;

namespace FclEx.RabbitMQ;

public abstract class MessageProcessor<TSettings> : IDisposable
    where TSettings : RmqSettings
{
    protected IMemoryBytesSerializer Serializer { get; }
    protected ILogger Logger { get; set; }
    [MemberNotNull(nameof(ExchangeName))]
    protected TSettings? Settings { get; set; }
    protected IConnection? Connection { get; set; }
    protected virtual bool DispatchConsumersAsync { get; } = false;
    protected virtual bool AutomaticRecoveryEnabled { get; } = true;
    protected string? ExchangeName => Settings?.Exchange.Name;
    protected ConnectionFactory? Factory { get; set; }
    protected bool IsDisposed { get; set; }

    protected MessageProcessor(IMemoryBytesSerializer? serializer = null, ILoggerFactory? loggerFactory = null)
    {
        Serializer = serializer ?? JsonMemoryBytesSerializer.Instance;
        Logger = (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger(GetType());
    }

    // ReSharper disable once InconsistentNaming
    protected virtual IEnumerable<LoggerProperty> GetLogProperties()
    {
        return Enumerable.Empty<LoggerProperty>();
    }

    [MemberNotNull(nameof(Connection), nameof(Settings))]
    public virtual void Init(TSettings settings)
    {
        Settings = Check.NotNull(settings);
        if (!Logger.IsNullOrNullLogger())
        {
            Logger = new PropertiesLogger(Logger, GetLogProperties());
        }

        var conStr = Settings.Connection.ToString();
        Factory = new ConnectionFactory
        {
            Uri = new Uri(conStr),
            DispatchConsumersAsync = DispatchConsumersAsync,
            AutomaticRecoveryEnabled = AutomaticRecoveryEnabled
        };
        Connection = Factory.CreateConnection();

        using var channel = Connection.CreateChannel();
        channel.Model.ExchangeDeclareWithAlternate(exchange: ExchangeName,
            type: Settings.Exchange.Type,
            durable: true,
            autoDelete: false,
            arguments: null!,
            isDelayed: Settings.Exchange.IsDelayed);
    }

    protected virtual void DisposeInternal()
    {
        Connection?.Close();
        Connection?.Dispose();
    }

    public void Dispose()
    {
        if (IsDisposed)
            return;

        IsDisposed = true;
        DisposeInternal();
    }
}