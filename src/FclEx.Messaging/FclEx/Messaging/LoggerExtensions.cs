using FclEx.Kafka;

namespace FclEx.Messaging;

public static class LoggerExtensions
{
    public static LogLevel GetLogLevel(this Error error)
    {
        if (error.IsFatal)
        {
            return LogLevel.Critical;
        }
        else if (error.IsError)
        {
            return error.IsLocalError
                ? LogLevel.Warning
                : LogLevel.Error;
        }
        else
        {
            return LogLevel.Warning;
        }
    }

    public static void KafkaError(this ILogger logger, string? topic, Error error, LogLevel? logLevel = null)
    {
        logLevel ??= error.GetLogLevel();

        using var log = new LoggerProperties(logger)
            .Push(LogPropertyNames.KafkaTopic, topic)
            .Push(nameof(KafkaErrorType), KafkaErrorType.FromErrorHandler)
            .Push(nameof(error.Code), error.Code.ToString())
            .Push(nameof(error.Reason), error.Reason)
            .Push(nameof(error.IsBrokerError), error.IsBrokerError)
            .Push(nameof(error.IsLocalError), error.IsLocalError)
            .Push(nameof(error.IsFatal), error.IsFatal);

        logger.Log(logLevel.Value, null, $"[Kafka ErrorHandler]{error.Reason}");
    }

    public static void KafkaError(this ILogger logger, string? topic, Exception ex, KafkaErrorType type)
    {
        using var log = new LoggerProperties(logger)
            .Push(LogPropertyNames.KafkaTopic, topic)
            .Push(nameof(KafkaErrorType), type);
        logger.LogError(ex, $"[Kafka Consumer]{ex.Message}");
    }

    public static void Kafka<TKey, TValue>(this ILogger logger, DeliveryResult<TKey, TValue> result)
    {
        var (level, msg) = result.Status switch
        {
            PersistenceStatus.NotPersisted => (LogLevel.Error, "Message was not persisted in Kafka"),
            PersistenceStatus.PossiblyPersisted => (LogLevel.Warning, "Message may have not been persisted in Kafka"),
            PersistenceStatus.Persisted => (LogLevel.Debug, "Message has been persisted in Kafka"),
            _ => throw new ArgumentOutOfRangeException(nameof(result.Status), result.Status, ""),
        };

        using var log = new LoggerProperties(logger, LogPropertyNames.KafkaTopic, result.Topic);
        logger.Log(level, msg);
    }

    public static (LogLevel, string) GetLogMessage(this PersistenceStatus status)
    {
        return status switch
        {
            PersistenceStatus.NotPersisted => (LogLevel.Error, "Message was not persisted in Kafka"),
            PersistenceStatus.PossiblyPersisted => (LogLevel.Warning, "Message may have not been persisted in Kafka"),
            PersistenceStatus.Persisted => (LogLevel.Debug, "Message has been persisted in Kafka"),
            _ => throw new ArgumentOutOfRangeException(nameof(status))
        };
    }

}