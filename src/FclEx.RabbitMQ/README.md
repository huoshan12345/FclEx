# FclEx.RabbitMQ

RabbitMQ publishers, consumers, and one-input/one-output conversion routers with automatic durable topology,
tracked publisher confirmations, quorum queue retries, dead lettering, and native delayed delivery.
This package uses RabbitMQ.Client directly and has no dependency on FclEx.Messaging or Kafka.

## Requirements

- RabbitMQ 4.3+ for normal operation and broker-native delayed retry.
- RabbitMQ 4.4+ and quorum destinations for initially delayed/scheduled messages.
- Compatible targets: netstandard2.0, net472, net8.0, net9.0, net10.0, and net11.0.

The package declares named, durable, nonexclusive quorum queues and ordinary durable exchanges.
It does not install plugins, change broker policies, delete conflicting resources, or migrate existing classic queues.
A single-node broker works for tests but does not provide node-failure tolerance.

## Publisher and Consumer

Prepare subscriptions before publishing so their queues and bindings already exist. Creating a Consumer prepares
topology; `RunAsync` separately starts consumption. `stoppingToken` below represents the application's shutdown token.

```csharp
using FclEx.RabbitMQ;
using RabbitMQ.Client;

await using var client = await RabbitMQClient.ConnectAsync(new ConnectionFactory
{
    HostName = "localhost",
    UserName = "app",
    Password = "<password>",
});

var exchange = new RabbitMQExchangeOptions { Name = "orders", Type = ExchangeType.Topic };

await using var consumer = await client.CreateConsumerAsync<string>(
    new RabbitMQConsumerOptions
    {
        Queue = new RabbitMQQueueOptions { Name = "orders.worker" },
        Bindings = [new RabbitMQBindingOptions { Exchange = exchange, RoutingKey = "created" }],
    },
    async (message, token) =>
    {
        await HandleOrderAsync(message.Body, token);
        return MessageHandlingResult.Completed;
    });

var consuming = consumer.RunAsync(stoppingToken);

await using var publisher = await client.CreatePublisherAsync<string>(
    new RabbitMQPublisherOptions { Exchange = exchange });

await publisher.PublishAsync(new PublishMessage<string>(
    "order-123", new RabbitMQPublishOptions { RoutingKey = "created" }, messageId: "order-123-created"));

await consuming;
```

Each Publisher is bound to one exchange and can publish concurrently: channel operations are serialized internally.
Publisher confirms and mandatory return tracking are always enabled. An unroutable publication throws rather than
being logged as success. Confirmation means broker acceptance, not that a handler has executed.
Timeouts or cancellation after sending can leave acceptance unknown; retries need stable message IDs and idempotent consumers.

Use `RabbitMQExchangeOptions.Default` to publish through the built-in default exchange and set the routing key to the queue name.
Its queue can be prepared using a Consumer with no bindings. For independent topology provisioning, the
`RabbitMQ.Client` namespace exposes `EnsureExchangeAsync`, `EnsureQueueAsync`, and `EnsureBindingAsync` extensions.

## Router

The Router prepares its entire input topology and target exchange before allowing consumption.
Its target subscribers must also be prepared before forwarding can succeed.

```csharp
await using var router = await client.CreateRouterAsync(
    new RabbitMQRouterOptions
    {
        Input = new RabbitMQConsumerOptions
        {
            Queue = new RabbitMQQueueOptions { Name = "orders.transform" },
            Bindings = [new RabbitMQBindingOptions { Exchange = exchange, RoutingKey = "created" }],
        },
        Target = new RabbitMQPublisherOptions
        {
            Exchange = new RabbitMQExchangeOptions { Name = "processed-orders" },
        },
        Output = new RabbitMQPublishOptions { RoutingKey = "processed" },
        ForwardedHeaderNames = ["tenant"],
    },
    new MessageConverter<string, string>((input, token) => ConvertOrderAsync(input.Body, token)));

await router.RunAsync(stoppingToken);
```

An `IMessageConverter<TInput,TOutput>` implementation can replace the delegate adapter. An optional output options selector
on `CreateRouterAsync` computes per-message routing keys or schedules. The input is acknowledged only after output publication
is confirmed. Publication and acknowledgement are not atomic: a crash between them can duplicate output.
Output preserves a nonempty input message ID, the correlation ID, and only allowlisted business headers.
Input scheduling, retry and death headers are not forwarded. Null output is a value when supported by the serializer, not a filter.
Source and target may be the same exchange, but output routing must not match the Router's own input bindings.

## Failure Handling and Shutdown

- `Completed` acknowledges successful work.
- `Retry` or an unclassified handler/Converter exception rejects with requeue, using the original quorum queue's native retry.
- `DeadLetter` or an input deserialization failure rejects without requeue.
- `Retry.Enabled=false` sends failed work directly to dead-letter topology.

Defaults are a one-second base delay, a 30-second cap and a broker delivery limit of 20.
Retry backoff is linear and bounded. The delivery limit counts broker-recognized failed deliveries,
including connection failures; it is not an exact count of business-handler invocations.
The implementation uses `basic.reject`, not `basic.nack`, so application failures contribute to the broker's failure counter.
There is no client-side retry sleep or republication to other subscribers.

For each input queue, the package creates `<queue>.dlx` and `<queue>.dead`, or explicitly configured names.
It enables at-least-once dead-letter transfer and reject-publish overflow. An unavailable dead-letter destination can cause
backpressure. Queue declarations and broker policies must agree; incompatible definitions fail instead of deleting existing queues.

Consumer concurrency and prefetch default to one. Each instance can run once. The task returned by `RunAsync` includes shutdown;
fatal subscription/settlement failures fault it, while requested cancellation completes normally.
Shutdown stops new deliveries and drains in-flight work. Callbacks must cooperate with cancellation.
Completed work can still be acknowledged while stopping; canceled/incomplete work is returned by channel closure.
After the shutdown timeout, channel cleanup proceeds in the background if a noncooperative callback prevents native disposal.
Disposing a role never removes broker topology. Disposing the client stops all its roles and disposes their shared connection.

## Initially Delayed Messages

On RabbitMQ 4.4+, schedule a message using either a relative `Delay` or an absolute `DeliverAt`:

```csharp
await publisher.PublishAsync(new PublishMessage<string>("reminder",
    new RabbitMQPublishOptions { RoutingKey = "created", Delay = TimeSpan.FromMinutes(5) }));
```

These settings are mutually exclusive. Negative delays are invalid; zero or past times publish immediately.
The publisher writes an Int64 `x-opt-delivery-time` UTC timestamp. The broker routes and stores the message immediately,
confirms it before it is due, and withholds delivery inside each quorum queue. Later bindings do not receive that earlier message.
Immediate messages are not held behind long-delay messages, and the due time means eligible delivery rather than exact execution time.

Relative delays are measured at the start of each publication call. Use a stable `DeliverAt` when retries must retain the same schedule.
Unsupported broker versions fail before sending a scheduled message. External bindings must also target quorum queues;
ordinary AMQP cannot enumerate and verify every external destination, and mandatory routing is not that verification.
Per-message `RetryAfter` is not offered: AMQP 0-9-1 reject cannot carry a replacement delivery time.

## Serialization and Metadata

`IMemoryBytesSerializer` is injectable when connecting. The default preserves strings as UTF-8 and encodes other values as JSON.
Default publications set their corresponding content type; custom serializers leave it unset rather than copying stale input metadata.
Bodies are retained by reference until serialization. Field-table metadata is copied recursively; public header getters return independent snapshots.
Transport-owned headers cannot be injected or forwarded as business headers.

## Tests

Unit tests run without a broker. For integration tests, configure ignored
`test/FclEx.RabbitMQ.Tests/appsettings.local.json` with:

```json
{
  "RabbitMQ": {
    "HostName": "127.0.0.1",
    "Port": 5678,
    "UserName": "fclex",
    "Password": "<test-password>"
  }
}
```

Equivalent environment variables use the prefix `FCLEX_RABBITMQ_TEST_`, for example
`FCLEX_RABBITMQ_TEST_RabbitMQ__HostName`. Without a host setting, broker integration tests are skipped.
The full integration suite requires RabbitMQ 4.4 and permission to create/delete its uniquely named test topology.

```powershell
dotnet test test/FclEx.RabbitMQ.Tests/FclEx.RabbitMQ.Tests.csproj -c Release -f net10.0
```

The implementation history and broker references are recorded in [DESIGN.md](DESIGN.md).
