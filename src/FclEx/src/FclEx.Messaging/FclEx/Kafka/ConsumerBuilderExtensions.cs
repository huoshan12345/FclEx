using static FclEx.Kafka.KafkaErrorType;

namespace FclEx.Kafka;

public static class ConsumerBuilderExtensions
{
    public static Task StartKafkaConsumer<T>(this ConsumerBuilder<string, T> builder, KafkaConsumerOptions<T> options)
    {
        Check.NotNull(options);
        Check.NotNull(options.Name);
        Check.NotNull(options.MessageHandler);
        Check.NotNull(options.Topic);

        var logger = options.Logger ?? NullLogger.Instance;
        return Task.Run(Consume);

        void ConfigureBuilder(ConsumerBuilder<string, T> builder)
        {
            builder.SetErrorHandler((_, error) => logger.KafkaError(options.Topic, error));
            if (options.Deserializer is { } deserializer)
            {
                builder.SetValueDeserializer(deserializer);
            }
        }

        async Task Consume()
        {
            ConfigureBuilder(builder);
            
            var consumer = builder.Build();

            consumer.Subscribe(options.Topic);

            while (!options.CancellationToken.IsCancellationRequested)
            {
                ConsumeResult<string, T>? result = null;
                try
                {
                    result = consumer.Consume(TimeSpan.FromSeconds(10));
                }
                catch (Exception ex)
                {
                    logger.KafkaError(options.Topic, ex, PollError);
                }

                if (result == null)
                    continue;

                try
                {
                    if (result.Message is not { } message)
                        continue;

                    await ConsumeMessage(message);
                }
                finally
                {
                    try
                    {
                        consumer.Commit(result);
                    }
                    catch (Exception ex)
                    {
                        logger.KafkaError(options.Topic, ex, CommitError);
                    }
                }
            }
        }

        async Task ConsumeMessage(Message<string, T> message)
        {
            var value = message.Value;
            try
            {
                await options.MessageHandler(value).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (options.ErrorHandler is { } errorHandler)
                {
                    try
                    {
                        await errorHandler(value, ex);
                    }
                    catch (Exception e)
                    {
                        logger.KafkaError(options.Topic, e, FromErrorHandler);
                    }
                }
                else
                {
                    logger.KafkaError(options.Topic, ex, ConsumeError);
                }
            }
        }
    }
}