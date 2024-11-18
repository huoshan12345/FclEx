using Microsoft.Extensions.DependencyInjection;

namespace FclEx.RabbitMQ;

public class MessageProcessorFactory(IServiceProvider Provider)
{
    protected virtual async Task<TProcessor> CreateAsync<TProcessor, TSettings>(TSettings settings)
        where TSettings : ProcessorSettings
        where TProcessor : IMessageProcessor<TSettings>
    {
        var processor = Provider.GetRequiredService<TProcessor>();
        await processor.InitializeAsync(settings);
        return processor;
    }

    public Task<T> CreatePublisherAsync<T>(PublisherSettings settings) where T : IMessagePublisher
    {
        return CreateAsync<T, PublisherSettings>(settings);
    }

    public Task<T> CreateConsumerAsync<T>(ConsumerSettings settings) where T : IMessageConsumer<T>
    {
        return CreateAsync<T, ConsumerSettings>(settings);
    }

    public Task<T> CreatePublisherAsync<T, TInput, TOutput>(RouterSettings settings) where T : IMessageRouter<TInput, TOutput>
    {
        return CreateAsync<T, RouterSettings>(settings);
    }
}