using FclEx.Messaging;
using static FclEx.Kafka.KafkaErrorType;

namespace FclEx.Kafka;

/// <summary>Runs Kafka consumers with offsets committed only after successful message handling.</summary>
public static class ConsumerConfigExtensions
{
    /// <summary>Consumes messages sequentially until cancellation or the first processing, polling, or commit failure.</summary>
    /// <typeparam name="T">The deserialized message value type. Message keys use UTF-8 strings.</typeparam>
    /// <param name="config">Consumer configuration, including bootstrap servers and a group ID. Copied before starting.</param>
    /// <param name="options">The topic, handlers, deserializer, logger, and cancellation token to use.</param>
    /// <returns>A task representing the entire consumption loop, including consumer shutdown.</returns>
    /// <remarks>
    /// Automatic commits and automatic offset storage are disabled in the configuration copy, even when enabled
    /// by the caller. Each record, including a tombstone, is committed only after its handler completes successfully.
    /// A handler failure is reported to the error handler, if supplied, and then faults the task without committing
    /// that record or consuming later records. An error-handler failure is logged without replacing the original error.
    /// Polling and commit failures also fault the task to prevent later commits from skipping an unresolved record.
    /// Cancellation stops polling normally; handlers must observe cancellation themselves to stop in-flight work.
    /// A handler that completes successfully is committed even if cancellation was requested during its execution.
    /// The consumer is closed and disposed on exit. Failed or uncommitted records can be replayed when the same group
    /// resumes, subject to its offset-reset policy and broker retention. Delivery is at least once: handlers must
    /// tolerate duplicates when processing succeeds but its offset cannot be committed.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The configuration, options, name, topic, or message handler is null.</exception>
    public static Task StartKafkaConsumer<T>(this ConsumerConfig config, KafkaConsumerOptions<T> options)
    {
        Check.NotNull(config);
        Check.NotNull(options);
        Check.NotNull(options.Name);
        var handler = Check.NotNull(options.MessageHandler);
        var topic = Check.NotNull(options.Topic);
        var errorHandler = options.ErrorHandler;
        var deserializer = options.Deserializer;
        var token = options.CancellationToken;
        var logger = options.Logger ?? NullLogger.Instance;
        var consumerConfig = new ConsumerConfig(config.ToDictionary(entry => entry.Key, entry => entry.Value))
        {
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false,
        };

        return Task.Run(Consume);

        async Task Consume()
        {
            var builder = new ConsumerBuilder<string, T>(consumerConfig)
                .SetErrorHandler((_, error) => logger.KafkaError(topic, error));
            if (deserializer is not null)
                builder.SetValueDeserializer(deserializer);

            using var consumer = builder.Build();
            var failed = false;
            try
            {
                consumer.Subscribe(topic);
                while (!token.IsCancellationRequested)
                {
                    ConsumeResult<string, T> result;
                    try
                    {
                        result = consumer.Consume(token);
                    }
                    catch (OperationCanceledException) when (token.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        logger.KafkaError(topic, ex, PollError);
                        throw;
                    }

                    if (result.IsPartitionEOF)
                        continue;

                    try
                    {
                        await handler(result.Message.Value).NoCapture();
                    }
                    catch (OperationCanceledException) when (token.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        logger.KafkaError(topic, ex, ConsumeError);
                        if (errorHandler is not null)
                        {
                            try
                            {
                                await errorHandler(result.Message.Value, ex).NoCapture();
                            }
                            catch (Exception errorHandlerException)
                            {
                                logger.KafkaError(topic, errorHandlerException, FromErrorHandler);
                            }
                        }
                        throw;
                    }

                    try
                    {
                        consumer.Commit(result);
                    }
                    catch (Exception ex)
                    {
                        logger.KafkaError(topic, ex, CommitError);
                        throw;
                    }
                }
            }
            catch
            {
                failed = true;
                throw;
            }
            finally
            {
                try
                {
                    consumer.Close();
                }
                catch (Exception ex) when (failed)
                {
                    logger.LogWarning(ex, "Kafka consumer shutdown failed after a consumption failure");
                }
            }
        }
    }
}
