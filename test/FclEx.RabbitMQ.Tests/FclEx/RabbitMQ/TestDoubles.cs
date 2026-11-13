using System.Text;

namespace FclEx.RabbitMQ;

internal sealed class TestDoubles
{
    internal Mock<IConnection> Connection { get; } = new();
    internal Mock<IChannel> Channel { get; } = new();
    internal RabbitMQClient Client { get; }
    internal IAsyncBasicConsumer? Consumer { get; private set; }
    internal TaskCompletionSource<bool> Subscribed { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    internal TestDoubles(string version = "4.4.0", bool automaticRecovery = false)
    {
        Connection.SetupGet(c => c.ServerProperties).Returns(new Dictionary<string, object?> { ["version"] = Encoding.UTF8.GetBytes(version) });
        Connection.SetupGet(c => c.IsOpen).Returns(true);
        Channel.SetupGet(c => c.IsOpen).Returns(true);
        Channel.Setup(c => c.BasicConsumeAsync(It.IsAny<string>(), false, It.IsAny<string>(), false, false,
                It.IsAny<IDictionary<string, object?>>(), It.IsAny<IAsyncBasicConsumer>(), It.IsAny<CancellationToken>()))
            .Returns(async (string _, bool _, string _, bool _, bool _, IDictionary<string, object?> _, IAsyncBasicConsumer consumer, CancellationToken token) =>
            {
                Consumer = consumer;
                await consumer.HandleBasicConsumeOkAsync("test-tag", token);
                Subscribed.TrySetResult(true);
                return "test-tag";
            });
        Client = new(Connection.Object, SerializerPresets.Utf8StringOrJson, NullLoggerFactory.Instance, automaticRecovery);
    }

    internal Task DeliverAsync(string body = "body", ulong tag = 1, BasicProperties? properties = null)
        => Consumer!.HandleBasicDeliverAsync("test-tag", tag, false, "input", "key", properties ?? new BasicProperties { MessageId = "input-id" },
            Encoding.UTF8.GetBytes(body), CancellationToken.None);

    internal ConsumerRunner<T> Runner<T>(Func<ReceivedMessage<T>, CancellationToken, Task<MessageHandlingResult>> handler,
        bool retry = true, TimeSpan? shutdownTimeout = null)
        => new(Client, Channel.Object, OptionsSnapshot.Consumer(new()
        {
            Queue = new() { Name = "input", Retry = new() { Enabled = retry } },
            ShutdownTimeout = shutdownTimeout ?? TimeSpan.FromSeconds(2),
        }), handler);
}
