using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using FclEx.Abp.RabbitMQ.Serializers;
using FclEx.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FclEx.Abp.RabbitMQ.MsgRoles;

public abstract class MsgProcessor<TSettings> : IDisposable
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

    protected MsgProcessor(IMemoryBytesSerializer? serializer = null, ILoggerFactory? loggerFactory = null)
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