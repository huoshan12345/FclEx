# FclEx.Messaging

Kafka and RabbitMQ helpers for FclEx.

## What Is Included

- Consumer, publisher, router, and processor abstractions.
- Message conversion helpers.
- RabbitMQ routing, queue, consumer, publisher, and processor options.
- Kafka helper types and option models.
- Retry metadata and logging helpers for messaging workflows.

## Usage Notes

- This package includes both RabbitMQ and Kafka dependencies.
- Keep broker-specific configuration in the appropriate option objects.
- Messaging tests may require local broker services.

## New RabbitMQ Package

The independent [FclEx.RabbitMQ package](../FclEx.RabbitMQ/README.md) provides the new publisher, consumer, router,
automatic topology declaration, and native Quorum Queue delay/retry APIs. Existing RabbitMQ APIs in this package remain unchanged.

## Kafka Consumer

Start the consumer from a `ConsumerConfig` and await the returned task for its entire lifetime:

```csharp
var config = new ConsumerConfig
{
    BootstrapServers = "localhost:9092",
    GroupId = "orders",
    AutoOffsetReset = AutoOffsetReset.Earliest,
};

await config.StartKafkaConsumer(new KafkaConsumerOptions<string>
{
    Name = "order-handler",
    Topic = "orders",
    Deserializer = Deserializers.Utf8,
    MessageHandler = HandleOrderAsync,
    CancellationToken = stoppingToken,
});
```

The consumer copies the configuration and disables automatic commits and automatic offset storage.
It handles records sequentially and commits each offset only after its handler succeeds, including tombstones.
A handler, deserialization, polling, or commit failure stops the consumer. Later records cannot commit past a failed record.
`ErrorHandler` reports message-handler failures; completing it does not authorize committing the failed record.
If that callback also fails, the returned task retains the original handler exception.

Cancellation interrupts polling. In-flight handlers must observe the token themselves: successful work is still committed,
while a handler that throws `OperationCanceledException` after cancellation leaves its record uncommitted.
The consumer closes and disposes on exit. Resume with the same group to replay uncommitted records, subject to broker retention
and the configured offset-reset policy; use `Earliest` if a group with no committed offset must replay existing records.
Handlers must tolerate duplicate delivery when processing succeeds but committing its offset fails.

The previous `ConsumerBuilder<string, T>.StartKafkaConsumer` entry point has been replaced by
`ConsumerConfig.StartKafkaConsumer`, so this helper can own and enforce its offset-commit configuration.

## Kafka Integration Tests

Configure `test/FclEx.Messaging.Tests/appsettings.local.json` (ignored by Git), or use environment variables
such as `FCLEX_TEST_Kafka__BootstrapServers`, `FCLEX_TEST_Kafka__SecurityProtocol`, `FCLEX_TEST_Kafka__SaslMechanism`,
`FCLEX_TEST_Kafka__SaslUsername`, and `FCLEX_TEST_Kafka__SaslPassword`.
The JSON uses the equivalent `Kafka` section with Confluent `ClientConfig` property names.
Without `Kafka:BootstrapServers`, only the Kafka integration tests are skipped.

The test user needs permission to create/delete topics and consumer groups, produce/consume messages, and query committed offsets.
Tests use unique topics and group IDs and remove them after each case.
When a broker advertises `localhost:9092`, forward local port 9092 to the broker through SSH before running the tests.
For an IPv4-only tunnel, set `Kafka:BrokerAddressFamily` to `V4` so advertised `localhost` addresses resolve consistently.

```powershell
dotnet test test/FclEx.Messaging.Tests/FclEx.Messaging.Tests.csproj -c Release -f net10.0 --filter "FullyQualifiedName~FclEx.Kafka.ConsumerConfigExtensionsTests"
```
