using Microsoft.Extensions.DependencyInjection;

namespace FclEx.RabbitMQ;

public class MessageProcessorFactory(IServiceProvider Provider)
{
    protected virtual async Task<TProcessor> CreateAsync<TProcessor, TSettings>(TSettings settings)
        where TSettings : RabbitMqProcessorOptions
        where TProcessor : IMessageProcessor<TSettings>
    {
        var processor = Provider.GetRequiredService<TProcessor>();
        await processor.InitializeAsync(settings);
        return processor;
    }

    public Task<T> CreatePublisherAsync<T>(RabbitMqPublisherOptions settings) where T : IMessagePublisher
    {
        return CreateAsync<T, RabbitMqPublisherOptions>(settings);
    }

    public Task<T> CreateConsumerAsync<T>(RabbitMqConsumerOptions settings) where T : IMessageConsumer<T>
    {
        return CreateAsync<T, RabbitMqConsumerOptions>(settings);
    }

    public Task<T> CreatePublisherAsync<T, TInput, TOutput>(RabbitMqRouterOptions settings) where T : IMessageRouter<TInput, TOutput>
    {
        return CreateAsync<T, RabbitMqRouterOptions>(settings);
    }
}